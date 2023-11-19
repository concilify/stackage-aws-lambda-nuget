using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Handlers;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class throwing_stream_handler_with_exception_middleware
   {
      private LambdaCompletion.Dictionary _responses;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         var functions = await TestHost.RunAsync(
            "my-function",
            new LambdaRequest("req-id", "AnyString"),
            configureLambdaListener: builder =>
            {
               builder.UseStartup<StartupWithExceptionHandling>();
               builder.UseHandler<ThrowingStreamLambdaHandler>();
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

         Assert.That(responseBody, Contains.Substring("\"errorType\": \"Exception\""));
         Assert.That(responseBody, Contains.Substring("\"errorMessage\": \"ThrowingStreamLambdaHandler failed\""));
      }
   }
}
