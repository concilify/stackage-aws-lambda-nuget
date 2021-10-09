using System.Collections.Concurrent;

namespace Stackage.Aws.Lambda.FakeRuntime.Model
{
   public record LambdaFunction(string Name)
   {
      public LambdaRequest.Queue Requests { get; } = new LambdaRequest.Queue();

      public LambdaResponse.Dictionary Responses { get; } = new LambdaResponse.Dictionary();

      public class Dictionary : ConcurrentDictionary<string, LambdaFunction>
      {
      }
   }
}
