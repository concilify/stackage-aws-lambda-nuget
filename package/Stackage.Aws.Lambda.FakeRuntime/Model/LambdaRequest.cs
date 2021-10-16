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

      public async Task WaitForCompletion()
      {
         _cancellationTokenSource = new CancellationTokenSource();

         try
         {
            await Task.Delay(TimeSpan.FromSeconds(60), _cancellationTokenSource.Token);
         }
         catch (OperationCanceledException) when (_cancellationTokenSource.IsCancellationRequested)
         {
         }
         finally
         {
            lock (_cancellationTokenSource)
            {
               _cancellationTokenSource.Dispose();
               _cancellationTokenSource = null;
            }
         }
      }

      public void NotifyCompletion()
      {
         _cancellationTokenSource?.Cancel();
      }
   }
}
