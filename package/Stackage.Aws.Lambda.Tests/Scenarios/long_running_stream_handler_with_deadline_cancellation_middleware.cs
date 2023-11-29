using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.Tests.Fakes.Model;
using Stackage.Aws.Lambda.Tests.Handlers;

namespace Stackage.Aws.Lambda.Tests.Scenarios
{
   public class long_running_stream_handler_with_deadline_cancellation_middleware
   {
      private IList<LogEntry> _logs;
      private LambdaCompletion.Dictionary _completions;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _logs = new List<LogEntry>();
         _completions = await TestHost.RunAsync(
            "my-function",
            new LambdaRequest("req-id", "AnyString"),
            configureLambdaListener: builder =>
            {
               builder.AddCapturingLogger(_logs);
               builder.UseStartup<StartupWithDeadlineCancellation>();
               builder.UseHandler<LongRunningStreamLambdaHandler>();
            },
            configureConfiguration: builder =>
            {
               builder.AddInMemoryCollection(new Dictionary<string, string>
               {
                  {"FAKERUNTIMEOPTIONS:DEADLINETIMEOUT", "00:00:03"}
               });
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

         Assert.That(responseBody, Contains.Substring("\"errorType\": \"CancellationError\""));
         Assert.That(responseBody, Contains.Substring("\"errorMessage\": \"The request was cancelled due to lack of remaining time; the handler responded promptly but may not have completed\""));
      }

      [Test]
      public void log_includes_exception()
      {
         var log = _logs.Single(l => l.CategoryName == "Stackage.Aws.Lambda.Middleware.DeadlineCancellationMiddleware");

         Assert.That(log.Exception, Is.InstanceOf<TaskCanceledException>());
         Assert.That(log.Exception.Message, Is.EqualTo("A task was canceled."));
      }
   }
}
