using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda
{
   internal class LambdaListener : ILambdaListener
   {
      private readonly ILambdaRuntime _lambdaRuntime;
      private readonly IServiceProvider _serviceProvider;
      private readonly PipelineDelegate _pipelineAsync;
      private readonly ILogger<LambdaListener> _logger;

      public LambdaListener(
         ILambdaRuntime lambdaRuntime,
         IServiceProvider serviceProvider,
         PipelineDelegate pipelineAsync,
         ILogger<LambdaListener> logger)
      {
         _lambdaRuntime = lambdaRuntime;
         _serviceProvider = serviceProvider;
         _pipelineAsync = pipelineAsync;
         _logger = logger;
      }

      public async Task ListenAsync(CancellationToken cancellationToken)
      {
         while (!cancellationToken.IsCancellationRequested)
         {
            try
            {
               using var invocation = await _lambdaRuntime.WaitForInvocationAsync(cancellationToken);

               await InvokeAndReplyAsync(invocation, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
         }
      }

      internal async Task InvokeAndReplyAsync(ILambdaInvocation invocation, CancellationToken cancellationToken)
      {
         using var _ = _logger.BeginScope(new Dictionary<string, object>
         {
            ["AwsRequestId"] = invocation.Context.AwsRequestId
         });

         using var scope = _serviceProvider.CreateScope();

         var lambdaResult = await InvokePipelineAsync(invocation, scope.ServiceProvider, cancellationToken);

         await lambdaResult.ExecuteResultAsync(invocation.Context, scope.ServiceProvider);
      }

      private async Task<ILambdaResult> InvokePipelineAsync(
         ILambdaInvocation invocation,
         IServiceProvider requestServices,
         CancellationToken cancellationToken)
      {
         var stopwatch = Stopwatch.StartNew();

         _logger.LogInformation("Handling request");

         try
         {
            var lambdaResult = await _pipelineAsync(invocation.InputStream, invocation.Context, requestServices, cancellationToken);

            _logger.LogInformation("Request handler completed {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            return lambdaResult;
         }
         catch (OperationCanceledException e) when (cancellationToken.IsCancellationRequested)
         {
            _logger.LogWarning(e, "Request handler cancelled {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            return new CancellationResult("The request was cancelled forcibly by the host");
         }
         catch (Exception e)
         {
            _logger.LogError(e, "Request handler failed {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);

            return new ExceptionResult(e);
         }
      }
   }
}
