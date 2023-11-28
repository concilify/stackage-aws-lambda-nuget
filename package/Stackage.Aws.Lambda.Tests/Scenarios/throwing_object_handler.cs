using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Handlers;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class throwing_object_handler
   {
      private LambdaCompletion.Dictionary _responses;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         var functions = await TestHost.RunAsync(
            "my-function",
            new LambdaRequest("req-id", "{\"value\":\"AnyString\"}"),
            configureLambdaListener: builder =>
            {
               builder.UseHandler<ThrowingObjectLambdaHandler, StringPoco>();
            });
         _responses = functions.Single().Value.CompletedRequests;
      }

      [Test]
      public void single_response_received()
      {
         Assert.That(_responses.Count, Is.EqualTo(1));
      }

      [Test]
      public void handler_received_request_and_returned_response()
      {
         var responseBody = _responses.Values.Single().ResponseBody;

         Assert.That(responseBody, Contains.Substring("\"errorType\": \"UnhandledError\""));
         Assert.That(responseBody, Contains.Substring("\"errorMessage\": \"The request failed due to an unhandled error; the handler may or may not have completed\""));
      }

      [Test]
      public void log_includes_exception()
      {
         // TODO: Include in other scenarios
         Assert.Fail();
      }
   }
}
