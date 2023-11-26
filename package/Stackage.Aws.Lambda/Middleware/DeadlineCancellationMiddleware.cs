using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Options;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Middleware
{
   public class DeadlineCancellationMiddleware : ILambdaMiddleware
   {
      private readonly IDeadlineCancellationInitializer _deadlineCancellationInitializer;
      private readonly DeadlineCancellationOptions _options;
      private readonly ILogger<DeadlineCancellationMiddleware> _logger;

      public DeadlineCancellationMiddleware(
         IDeadlineCancellationInitializer deadlineCancellationInitializer,
         IOptions<DeadlineCancellationOptions> options,
         ILogger<DeadlineCancellationMiddleware> logger)
      {
         _deadlineCancellationInitializer = deadlineCancellationInitializer;
         _options = options.Value;
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         Stream inputStream,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate next,
         CancellationToken cancellationToken)
      {
         // Pipeline if given softLimitMs to cancel, if it doesn't within that period the function has hardLimitMs to report an indeterminate error
         var hardLimitMs = RemainingTimeLessInterval(context, _options.HardInterval);
         var softLimitMs = RemainingTimeLessInterval(context, _options.HardInterval + _options.SoftInterval);

         if (softLimitMs <= 0)
         {
            const string message = "The request was shortcut due to lack of remaining time; the handler was not started";

            _logger.LogWarning(message);

            return new CancellationResult(message);
         }

         using var softLimitExpired = new CancellationTokenSource(softLimitMs);
         using var cancelledOrSoftLimitExpired = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, softLimitExpired.Token);

         _deadlineCancellationInitializer.Initialize(cancelledOrSoftLimitExpired.Token);

         var nextTask = next(inputStream, context, requestServices, cancelledOrSoftLimitExpired.Token);
         var hardLimitExpiredTask = Task.Delay(hardLimitMs, cancellationToken);

         var completedTask = await Task.WhenAny(nextTask, hardLimitExpiredTask);

         if (completedTask == hardLimitExpiredTask)
         {
            // Will bubble exception up to caller if task was cancelled
            await hardLimitExpiredTask;

            const string message = "The request was cancelled due to lack of remaining time but failed to respond; it may or may not have completed";

            _logger.LogError(message);

            return new CancellationResult(message);
         }

         try
         {
            // Throw exception or return result
            return await nextTask;
         }
         catch (OperationCanceledException e) when (softLimitExpired.IsCancellationRequested)
         {
            const string message = "The request was cancelled due to lack of remaining time and responded promptly; it may or may not have completed";

            _logger.LogWarning(e, message);

            return new CancellationResult(message);
         }
      }

      private static int RemainingTimeLessInterval(ILambdaContext context, TimeSpan interval)
      {
         return Math.Max((int) context.RemainingTime.Subtract(interval).TotalMilliseconds, 0);
      }
   }
}
