using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   internal class LambdaListener : ILambdaListener
   {
      private readonly ILambdaRuntime _lambdaRuntime;
      private readonly IServiceProvider _serviceProvider;
      private readonly PipelineDelegate _pipelineAsync;
      private readonly ILambdaSerializer _serializer;
      private readonly ILogger<LambdaListener> _logger;

      public LambdaListener(
         ILambdaRuntime lambdaRuntime,
         IServiceProvider serviceProvider,
         PipelineDelegate pipelineAsync,
         ILambdaSerializer serializer,
         ILogger<LambdaListener> logger)
      {
         _lambdaRuntime = lambdaRuntime;
         _serviceProvider = serviceProvider;
         _pipelineAsync = pipelineAsync;
         _serializer = serializer;
         _logger = logger;
      }

      public async Task ListenAsync(CancellationToken cancellationToken)
      {
         while (!cancellationToken.IsCancellationRequested)
         {
            try
            {
               await WaitAndInvokeNextAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
         }
      }

      private async Task WaitAndInvokeNextAsync(CancellationToken cancellationToken)
      {
         using var invocation = await _lambdaRuntime.WaitForInvocationAsync(cancellationToken);

         var stopwatch = Stopwatch.StartNew();

         using var _ = _logger.BeginScope(new Dictionary<string, object>
         {
            ["AwsRequestId"] = invocation.Context.AwsRequestId
         });

         _logger.LogInformation("Handling request");

         Stream outputStream;

         try
         {
            using var scope = _serviceProvider.CreateScope();

            var lambdaResult = await _pipelineAsync(invocation.InputStream, invocation.Context, scope.ServiceProvider);

            outputStream = lambdaResult.SerializeResult(_serializer, invocation.Context);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Request handler failed {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            await _lambdaRuntime.ReplyWithInvocationFailureAsync(e, invocation.Context, cancellationToken);
            return;
         }

         try
         {
            _logger.LogInformation("Request handler completed {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            await _lambdaRuntime.ReplyWithInvocationSuccessAsync(outputStream, invocation.Context, cancellationToken);
         }
         finally
         {
            await outputStream.DisposeAsync();
         }
      }
   }
}
