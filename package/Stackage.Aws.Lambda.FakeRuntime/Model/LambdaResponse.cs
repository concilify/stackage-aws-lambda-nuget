using System.Collections.Concurrent;

namespace Stackage.Aws.Lambda.FakeRuntime.Model
{
   public record LambdaResponse(string AwsRequestId, string Body, bool Success)
   {
      public class Dictionary : ConcurrentDictionary<string, LambdaResponse>
      {
      }
   }
}
