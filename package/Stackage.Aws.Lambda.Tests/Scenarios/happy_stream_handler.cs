using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Handlers;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class happy_stream_handler
   {
      private LambdaCompletion.Dictionary _completions;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _completions = await TestHost.RunAsync(
            "my-function",
            new LambdaRequest("req-id", "AnyString"),
            configureLambdaListener: builder =>
            {
               builder.UseHandler<DecorateStreamLambdaHandler>();
            });
      }

      [Test]
      public void single_response_received()
      {
         Assert.That(_completions.Count, Is.EqualTo(1));
      }

      [Test]
      public void handler_received_request_and_returned_response()
      {
         Assert.That(_completions.Values.Single().ResponseBody, Is.EqualTo("[AnyString]"));
      }
   }
}
