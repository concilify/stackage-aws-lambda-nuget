using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Stackage.Aws.Lambda.FakeRuntime.Components;

namespace Stackage.Aws.Lambda.FakeRuntime.Model
{
   public record LambdaRequest(string AwsRequestId, string Body)
   {
      private CancellationTokenSource? _cancellationTokenSource;

      public class Queue : BlockingQueue<LambdaRequest>
      {
      }

      public class Dictionary : ConcurrentDictionary<string, LambdaRequest>
      {
      }

      // Wait indefinitely until either cancellationToken is triggered (return false) or
      // NotifyCompletion has been called (return true)
      public async Task<bool> WaitForCompletion(CancellationToken cancellationToken)
      {
         _cancellationTokenSource = new CancellationTokenSource();

         var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _cancellationTokenSource.Token, cancellationToken);

         try
         {
            await Task.Delay(Timeout.Infinite, combinedTokenSource.Token);
         }
         catch (OperationCanceledException)
         {
            Console.WriteLine($"foo {_cancellationTokenSource.IsCancellationRequested}");

            return _cancellationTokenSource.IsCancellationRequested;
         }
         finally
         {
            lock (_cancellationTokenSource)
            {
               _cancellationTokenSource.Dispose();
               _cancellationTokenSource = null;
            }
         }

         return false;
      }

      public void NotifyCompletion()
      {
         _cancellationTokenSource?.Cancel();
      }
   }
}
