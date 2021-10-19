using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Handlers;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class long_running_stream_handler_with_exception_middleware
   {
      private LambdaCompletion.Dictionary _responses;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         var functions = await TestHost.RunAsync(
            builder =>
            {
               builder.UseStartup<StartupWithDeadlineCancellation<Stream>>();
               builder.UseHandler<LongRunningStreamLambdaHandler>();
            },
            builder =>
            {
               builder.AddInMemoryCollection(new Dictionary<string, string>
               {
                  {"FAKERUNTIMEOPTIONS:DEADLINETIMEOUT", "00:00:01"}
               });
            },
            "my-function",
            new LambdaRequest("req-id", "AnyString"));
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
         Assert.That(_responses.Values.Single().ResponseBody, Is.EqualTo("Client Closed Request"));
      }
   }
}