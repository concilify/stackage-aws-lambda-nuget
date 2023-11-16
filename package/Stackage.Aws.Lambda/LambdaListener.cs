using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   internal class LambdaListener : ILambdaListener
   {
      private readonly IRuntimeApiClient _runtimeApiClient;
      private readonly IServiceProvider _serviceProvider;
      private readonly PipelineDelegate _pipelineAsync;
      private readonly ILambdaSerializer _serializer;
      private readonly ILogger<LambdaListener> _logger;

      public LambdaListener(
         IRuntimeApiClient runtimeApiClient,
         IServiceProvider serviceProvider,
         PipelineDelegate pipelineAsync,
         ILambdaSerializer serializer,
         ILogger<LambdaListener> logger)
      {
         _runtimeApiClient = runtimeApiClient;
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
         using var invocation = await _runtimeApiClient.GetNextInvocationAsync(cancellationToken);

         var stopwatch = Stopwatch.StartNew();

         using var _ = _logger.BeginScope(new Dictionary<string, object> {{"AwsRequestId", invocation.LambdaContext.AwsRequestId}});

         _logger.LogInformation("Handling request");

         Stream response;

         try
         {
            using var scope = _serviceProvider.CreateScope();

            var lambdaResult = await _pipelineAsync(invocation.InputStream, invocation.LambdaContext, scope.ServiceProvider);

            response = lambdaResult.SerializeResult(_serializer, invocation.LambdaContext);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Request handler failed {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            await _runtimeApiClient.ReportInvocationErrorAsync(invocation.LambdaContext.AwsRequestId, e, CancellationToken.None);
            return;
         }

         try
         {
            _logger.LogInformation("Request handler completed {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            await _runtimeApiClient.SendResponseAsync(invocation.LambdaContext.AwsRequestId, response, CancellationToken.None);
         }
         finally
         {
            await response.DisposeAsync();
         }
      }
   }
}
