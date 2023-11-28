using System;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class CancellationResultExecutorTests
{
   [Test]
   public async Task execute_invokes_report_invocation_error()
   {
      var result = new CancellationResult("ArbitraryMessage");

      var lambdaRuntime = A.Fake<ILambdaRuntime>();
      var resultExecutor = new CancellationResult.Executor(lambdaRuntime);
      var serviceProvider = ServiceProviderFake.Returns<ILambdaResultExecutor<CancellationResult>>(resultExecutor);
      var context = LambdaContextFake.With(awsRequestId: "ArbitraryRequestId");

      await result.ExecuteResultAsync(context, serviceProvider);

      A.CallTo(() => lambdaRuntime.ReplyWithInvocationFailureAsync(
            A<Exception>.That.Matches(e => e is TaskCanceledException && e.Message == "ArbitraryMessage"),
            context))
         .MustHaveHappenedOnceExactly();
   }
}
