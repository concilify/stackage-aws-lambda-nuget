using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Handlers;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class happy_object_handler
   {
      private LambdaCompletion.Dictionary _responses;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         var functions = await TestHost.RunAsync<StringPoco>(
            builder =>
            {
               builder.UseHandler<DecorateObjectLambdaHandler, StringPoco>();
            },
            null,
            "my-function",
            new LambdaRequest("req-id", "{\"value\":\"AnyString\"}"));
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
         Assert.That(_responses.Values.Single().ResponseBody, Is.EqualTo("{\"value\":\"[AnyString]\"}"));
      }
   }
}
