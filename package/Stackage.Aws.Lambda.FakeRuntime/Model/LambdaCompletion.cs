using System.Collections.Concurrent;

namespace Stackage.Aws.Lambda.FakeRuntime.Model
{
   public record LambdaCompletion(string AwsRequestId, string RequestBody, string ResponseBody, bool Success)
   {
      public class Dictionary : ConcurrentDictionary<string, LambdaCompletion>
      {
      }
   }
}
