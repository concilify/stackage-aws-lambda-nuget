using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class LambdaListener<TRequest> : ILambdaListener<TRequest>
   {
      private readonly ILambdaPipelineBuilder<TRequest> _pipelineBuilder;
      private readonly IRuntimeApiClient _runtimeApiClient;
      private readonly IRequestHandler<TRequest> _requestHandler;
      private readonly LambdaPipelineBuilderOptions<TRequest> _options;
      private readonly ILogger<LambdaListener<TRequest>> _logger;

      public LambdaListener(
         ILambdaPipelineBuilder<TRequest> pipelineBuilder,
         IRuntimeApiClient runtimeApiClient,
         IRequestHandler<TRequest> requestHandler,
         IOptions<LambdaPipelineBuilderOptions<TRequest>> options,
         ILogger<LambdaListener<TRequest>> logger)
      {
         _pipelineBuilder = pipelineBuilder;
         _runtimeApiClient = runtimeApiClient;
         _requestHandler = requestHandler;
         _options = options.Value;
         _logger = logger;
      }

      public async Task ListenAsync(CancellationToken cancellationToken)
      {
         var pipelineAsync = InitialisePipeline();

         while (!cancellationToken.IsCancellationRequested)
         {
            try
            {
               await WaitAndInvokeNextAsync(pipelineAsync, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
         }
      }

      private PipelineDelegate<TRequest> InitialisePipeline()
      {
         _logger.LogDebug("Initialising pipeline");

         _options.ConfigurePipeline?.Invoke(_pipelineBuilder);

         var pipelineAsync = _pipelineBuilder.Build();

         _logger.LogDebug("Pipeline initialised");

         return pipelineAsync;
      }

      private async Task WaitAndInvokeNextAsync(PipelineDelegate<TRequest> pipelineAsync, CancellationToken cancellationToken)
      {
         using var invocation = await _runtimeApiClient.GetNextInvocationAsync(cancellationToken);

         using var _ = _logger.BeginScope(new Dictionary<string, object> {{"AwsRequestId", invocation.LambdaContext.AwsRequestId}});

         _logger.LogInformation("Handling request");

         var stopwatch = Stopwatch.StartNew();

         Stream response;

         try
         {
            response = await _requestHandler.HandleAsync(invocation.InputStream, invocation.LambdaContext, pipelineAsync);
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
