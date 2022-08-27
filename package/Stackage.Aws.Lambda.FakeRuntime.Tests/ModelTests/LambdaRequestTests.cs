using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Model;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ModelTests
{
   public class LambdaRequestTests
   {
      [Test]
      public async Task wait_for_completion_returns_false_when_request_is_aborted()
      {
         var testSubject = new LambdaRequest("AnyAwsRequestId", "AnyBody");

         var cancellationTokenSource = new CancellationTokenSource();

         var waitTask = testSubject.WaitForCompletion(cancellationTokenSource.Token);

         cancellationTokenSource.Cancel();

         Assert.That(await waitTask, Is.False);
      }

      [Test]
      public async Task wait_for_completion_returns_true_when_completion_notified()
      {
         var testSubject = new LambdaRequest("AnyAwsRequestId", "AnyBody");

         var waitTask = testSubject.WaitForCompletion(new CancellationTokenSource().Token);

         testSubject.NotifyCompletion();

         Assert.That(await waitTask, Is.True);
      }
   }
}
