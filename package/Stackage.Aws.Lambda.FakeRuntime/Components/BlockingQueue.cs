using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Stackage.Aws.Lambda.FakeRuntime.Components
{
   public class BlockingQueue<T>
   {
      private readonly ConcurrentQueue<T> _queue;
      private readonly SemaphoreSlim _semaphore;

      public BlockingQueue()
      {
         _queue = new ConcurrentQueue<T>();
         _semaphore = new SemaphoreSlim(0);
      }

      public int Count => _queue.Count;

      public void Enqueue(T item)
      {
         _queue.Enqueue(item);
         _semaphore.Release();
      }

      public async Task<T> DequeueAsync(CancellationToken cancellationToken = default)
      {
         while (true)
         {
            await _semaphore.WaitAsync(cancellationToken);

            if (_queue.TryDequeue(out var item))
            {
               return item;
            }
         }
      }
   }
}
