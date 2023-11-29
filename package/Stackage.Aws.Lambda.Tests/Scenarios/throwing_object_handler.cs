using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Fakes.Model;
using Stackage.Aws.Lambda.Tests.Handlers;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class throwing_object_handler
   {
      private IList<LogEntry> _logs;
      private LambdaCompletion.Dictionary _completions;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _logs = new List<LogEntry>();
         _completions = await TestHost.RunAsync(
            "my-function",
            new LambdaRequest("req-id", "{\"value\":\"AnyString\"}"),
            configureLambdaListener: builder =>
            {
               builder.AddCapturingLogger(_logs);
               builder.UseHandler<ThrowingObjectLambdaHandler, StringPoco>();
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
         var responseBody = _completions.Values.Single().ResponseBody;

         Assert.That(responseBody, Contains.Substring("\"errorType\": \"UnhandledError\""));
         Assert.That(responseBody, Contains.Substring("\"errorMessage\": \"The request failed due to an unhandled error; the handler may or may not have completed\""));
      }

      [Test]
      public void log_includes_exception()
      {
         var log = _logs.Single(l => l.CategoryName == "Stackage.Aws.Lambda.Middleware.InvocationMiddleware");

         Assert.That(log.Exception, Is.InstanceOf<CustomException>());
         Assert.That(log.Exception.Message, Is.EqualTo("ThrowingObjectLambdaHandler failed"));
      }
   }
}
