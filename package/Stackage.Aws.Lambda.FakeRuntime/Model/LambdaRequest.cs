using Stackage.Aws.Lambda.FakeRuntime.Components;

namespace Stackage.Aws.Lambda.FakeRuntime.Model
{
   public record LambdaRequest(string AwsRequestId, string Body)
   {
      public class Queue : BlockingQueue<LambdaRequest>
      {
      }
   }
}
