using System.Collections.Concurrent;

namespace Stackage.Aws.Lambda.FakeRuntime.Model
{
   public record LambdaFunction(string Name)
   {
      public LambdaRequest.Queue QueuedRequests { get; } = new LambdaRequest.Queue();

      public LambdaRequest.Dictionary InFlightRequests { get; } = new LambdaRequest.Dictionary();

      public LambdaCompletion.Dictionary CompletedRequests { get; } = new LambdaCompletion.Dictionary();

      public class Dictionary : ConcurrentDictionary<string, LambdaFunction>
      {
      }
   }
}
