using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Handlers;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class happy_stream_handler_with_exception_middleware
   {
      private LambdaResponse.Dictionary _responses;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         var functions = await TestHost.RunAsync(
            builder =>
            {
               builder.UseStartup<StartupWithExceptionHandling<Stream>>();
               builder.UseHandler<DecorateStreamLambdaHandler>();
            },
            "my-function",
            new LambdaRequest("req-id", "AnyString"));
         _responses = functions.Single().Value.Responses;
      }

      [Test]
      public void single_response_received()
      {
         Assert.That(_responses.Count, Is.EqualTo(1));
      }

      [Test]
      public void handler_received_request_and_returned_response()
      {
         Assert.That(_responses.Values.Single().Body, Is.EqualTo("[AnyString]"));
      }
   }
}
