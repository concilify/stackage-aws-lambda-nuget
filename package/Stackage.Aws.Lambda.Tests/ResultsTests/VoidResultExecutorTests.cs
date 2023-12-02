using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class VoidResultExecutorTests
{
   [Test]
   public async Task execute_invokes_send_response()
   {
      var result = new VoidResult();

      var lambdaRuntime = LambdaRuntimeFake.ValidFake();
      var resultExecutor = new VoidResult.Executor(lambdaRuntime);
      var serviceProvider = ServiceProviderFake.Returns<ILambdaResultExecutor<VoidResult>>(resultExecutor);
      var context = LambdaContextFake.With(awsRequestId: "ArbitraryRequestId");

      await result.ExecuteResultAsync(context, serviceProvider);

      A.CallTo(() => lambdaRuntime.ReplyWithInvocationSuccessAsync(null, context))
         .MustHaveHappenedOnceExactly();
   }
}
