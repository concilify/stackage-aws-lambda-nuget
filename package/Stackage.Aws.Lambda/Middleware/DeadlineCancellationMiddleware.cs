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
         CancellationToken requestAborted)
      {
         // Pipeline is given context.RemainingTime less HardInterval and SoftInterval to complete
         // Cancellation is triggered if it hasn't completed, giving it SoftInterval to cancel
         // The middleware intervenes if it hasn't cancelled, giving HardInterval for the caller to reply to the lambda runtime

         var timeBeforeHardLimitMs = RemainingTimeLessInterval(context, _options.HardInterval);
         var timeBeforeCancellationMs = RemainingTimeLessInterval(context, _options.HardInterval + _options.SoftInterval);

         if (timeBeforeCancellationMs <= 0)
         {
            return ShortcutCancellationResult();
         }

         using var remainingTimeExpired = new CancellationTokenSource(timeBeforeCancellationMs);
         using var remainingTimeExpiredOrRequestAborted = CancellationTokenSource.CreateLinkedTokenSource(
            remainingTimeExpired.Token, requestAborted);

         _deadlineCancellationInitializer.Initialize(remainingTimeExpiredOrRequestAborted.Token);

         var nextTask = next(inputStream, context, requestServices, remainingTimeExpiredOrRequestAborted.Token);
         var hardLimitExpiredTask = Task.Delay(timeBeforeHardLimitMs, requestAborted);

         var completedTask = await Task.WhenAny(nextTask, hardLimitExpiredTask);

         if (completedTask == hardLimitExpiredTask)
         {
            try
            {
               await hardLimitExpiredTask;

               return HardLimitCancellationResult();
            }
            catch (OperationCanceledException e) when (requestAborted.IsCancellationRequested)
            {
               return CancelledByHostCancellationResult(e);
            }
         }

         try
         {
            return await nextTask;
         }
         catch (OperationCanceledException e) when (remainingTimeExpired.IsCancellationRequested)
         {
            return RemainingTimeExpiredCancellationResult(e);
         }
         catch (OperationCanceledException e) when (requestAborted.IsCancellationRequested)
         {
            return CancelledByHostCancellationResult(e);
         }
      }

      private static int RemainingTimeLessInterval(ILambdaContext context, TimeSpan interval)
      {
         return Math.Max((int) context.RemainingTime.Subtract(interval).TotalMilliseconds, 0);
      }

      private CancellationResult ShortcutCancellationResult()
         => CreateCancellationResult("The request was shortcut due to lack of remaining time; the handler was not invoked");

      private CancellationResult RemainingTimeExpiredCancellationResult(Exception exception)
         => CreateCancellationResult("The request was cancelled due to lack of remaining time and responded promptly; it may or may not have completed", exception);

      private CancellationResult HardLimitCancellationResult()
         => CreateCancellationResult("The request was cancelled due to lack of remaining time but failed to respond; it may or may not have completed");

      private CancellationResult CancelledByHostCancellationResult(Exception exception)
         => CreateCancellationResult("The request was cancelled by the host; it may or may not have completed", exception);

      private CancellationResult CreateCancellationResult(string message, Exception? exception = null)
      {
         _logger.LogError(exception, message);

         return new CancellationResult(message);
      }
   }
}
