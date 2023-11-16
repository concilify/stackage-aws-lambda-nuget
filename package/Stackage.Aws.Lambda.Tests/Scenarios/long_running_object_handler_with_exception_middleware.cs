using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Handlers;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class long_running_object_handler_with_exception_middleware
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
               builder.UseStartup<StartupWithDeadlineCancellation>();
               builder.UseHandler<LongRunningObjectLambdaHandler, StringPoco>();
            },
            configureConfiguration: builder =>
            {
               builder.AddInMemoryCollection(new Dictionary<string, string>
               {
                  {"FAKERUNTIMEOPTIONS:DEADLINETIMEOUT", "00:00:01"}
               });
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
         Assert.That(_responses.Values.Single().ResponseBody, Is.EqualTo("Client Closed Request"));
      }
   }
}
