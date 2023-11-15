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
using Microsoft.Extensions.Options;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class LambdaListener : ILambdaListener
   {
      private readonly ILambdaPipelineBuilder _pipelineBuilder;
      private readonly IRuntimeApiClient _runtimeApiClient;
      private readonly IServiceProvider _serviceProvider;
      private readonly ILambdaSerializer _serializer;
      private readonly LambdaPipelineBuilderOptions _options;
      private readonly ILogger<LambdaListener> _logger;

      public LambdaListener(
         ILambdaPipelineBuilder pipelineBuilder,
         IRuntimeApiClient runtimeApiClient,
         IServiceProvider serviceProvider,
         IOptions<LambdaPipelineBuilderOptions> options,
         ILambdaSerializer serializer,
         ILogger<LambdaListener> logger)
      {
         _pipelineBuilder = pipelineBuilder;
         _runtimeApiClient = runtimeApiClient;
         _serviceProvider = serviceProvider;
         _serializer = serializer;
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

      private PipelineDelegate InitialisePipeline()
      {
         _logger.LogDebug("Initialising pipeline");

         _options.ConfigurePipeline?.Invoke(_pipelineBuilder);

         var pipelineAsync = _pipelineBuilder.Build();

         _logger.LogDebug("Pipeline initialised");

         return pipelineAsync;
      }

      private async Task WaitAndInvokeNextAsync(PipelineDelegate pipelineAsync, CancellationToken cancellationToken)
      {
         using var invocation = await _runtimeApiClient.GetNextInvocationAsync(cancellationToken);

         var stopwatch = Stopwatch.StartNew();

         using var _ = _logger.BeginScope(new Dictionary<string, object> {{"AwsRequestId", invocation.LambdaContext.AwsRequestId}});

         _logger.LogInformation("Handling request");

         Stream response;

         try
         {
            using var scope = _serviceProvider.CreateScope();

            var lambdaResult = await pipelineAsync(invocation.InputStream, invocation.LambdaContext, scope.ServiceProvider);

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
