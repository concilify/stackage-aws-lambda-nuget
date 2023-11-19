using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Middleware
{
   public class DeadlineCancellationMiddleware : ILambdaMiddleware
   {
      private readonly IConfiguration _configuration;
      private readonly IDeadlineCancellationInitializer _deadlineCancellationInitializer;
      private readonly ILogger<DeadlineCancellationMiddleware> _logger;

      public DeadlineCancellationMiddleware(
         IConfiguration configuration,
         IDeadlineCancellationInitializer deadlineCancellationInitializer,
         ILogger<DeadlineCancellationMiddleware> logger)
      {
         _configuration = configuration;
         _deadlineCancellationInitializer = deadlineCancellationInitializer;
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         Stream inputStream,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate next,
         CancellationToken cancellationToken)
      {
         var effectiveRemainingTimeMs = GetEffectiveRemainingTimeMs(context);

         using var remainingTimeExpired = new CancellationTokenSource(effectiveRemainingTimeMs);
         using var abortedOrExpired = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, remainingTimeExpired.Token);

         _deadlineCancellationInitializer.Initialize(abortedOrExpired.Token);

         try
         {
            return await next(inputStream, context, requestServices, abortedOrExpired.Token);
         }
         catch (OperationCanceledException e) when (remainingTimeExpired.IsCancellationRequested)
         {
            const string message = "The request was cancelled due to expiry of remaining time";

            _logger.LogWarning(e, message);

            return new CancellationResult(message);
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
