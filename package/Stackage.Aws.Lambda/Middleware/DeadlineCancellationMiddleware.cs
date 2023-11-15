using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Middleware
{
   public class DeadlineCancellationMiddleware : ILambdaMiddleware
   {
      private readonly HostOptions _hostOptions;
      private readonly IDeadlineCancellationInitializer _deadlineCancellationInitializer;
      private readonly ILambdaResultFactory _resultFactory;
      private readonly ILogger<DeadlineCancellationMiddleware> _logger;

      public DeadlineCancellationMiddleware(
         IOptions<HostOptions> hostOptions,
         IDeadlineCancellationInitializer deadlineCancellationInitializer,
         ILambdaResultFactory resultFactory,
         ILogger<DeadlineCancellationMiddleware> logger)
      {
         _hostOptions = hostOptions.Value;
         _deadlineCancellationInitializer = deadlineCancellationInitializer;
         _resultFactory = resultFactory;
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         Stream request,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate next)
      {
         var effectiveRemainingTimeMs = Math.Max((int) context.RemainingTime.Subtract(_hostOptions.ShutdownTimeout).TotalMilliseconds, 0);

         using var requestAborted = new CancellationTokenSource(effectiveRemainingTimeMs);

         _deadlineCancellationInitializer.Initialize(requestAborted.Token);

         try
         {
            return await next(request, context, requestServices);
         }
         catch (OperationCanceledException) when (requestAborted.IsCancellationRequested)
         {
            _logger.LogWarning("The request was aborted gracefully");

            return _resultFactory.RemainingTimeExpired();
         }
      }
   }
}
