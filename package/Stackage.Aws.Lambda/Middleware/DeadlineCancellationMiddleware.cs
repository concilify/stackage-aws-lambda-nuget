using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Middleware
{
   public class DeadlineCancellationMiddleware : ILambdaMiddleware
   {
      private readonly IConfiguration _configuration;
      private readonly IDeadlineCancellationInitializer _deadlineCancellationInitializer;
      private readonly ILambdaResultFactory _resultFactory;
      private readonly ILogger<DeadlineCancellationMiddleware> _logger;

      public DeadlineCancellationMiddleware(
         IConfiguration configuration,
         IDeadlineCancellationInitializer deadlineCancellationInitializer,
         ILambdaResultFactory resultFactory,
         ILogger<DeadlineCancellationMiddleware> logger)
      {
         _configuration = configuration;
         _deadlineCancellationInitializer = deadlineCancellationInitializer;
         _resultFactory = resultFactory;
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         Stream inputStream,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate next)
      {
         var effectiveRemainingTimeMs = GetEffectiveRemainingTimeMs(context);

         using var requestAborted = new CancellationTokenSource(effectiveRemainingTimeMs);

         _deadlineCancellationInitializer.Initialize(requestAborted.Token);

         try
         {
            return await next(inputStream, context, requestServices);
         }
         catch (OperationCanceledException) when (requestAborted.IsCancellationRequested)
         {
            _logger.LogWarning("The request was aborted gracefully");

            return _resultFactory.RemainingTimeExpired();
         }
      }

      private int GetEffectiveRemainingTimeMs(ILambdaContext context)
      {
         return Math.Max((int) context.RemainingTime.Subtract(GetShutdownTimeout()).TotalMilliseconds, 0);
      }

      private TimeSpan GetShutdownTimeout()
      {
         const int defaultShutdownTimeoutMs = 5000;

         var shutdownTimeoutMs = _configuration["ShutdownTimeoutMs"];

         if (!string.IsNullOrEmpty(shutdownTimeoutMs) && int.TryParse(shutdownTimeoutMs, NumberStyles.None, CultureInfo.InvariantCulture, out var milliseconds))
         {
            return TimeSpan.FromMilliseconds(milliseconds);
         }

         return TimeSpan.FromMilliseconds(defaultShutdownTimeoutMs);
      }
   }
}
