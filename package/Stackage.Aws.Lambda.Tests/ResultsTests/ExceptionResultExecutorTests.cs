using System;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class ExceptionResultExecutorTests
{
   [Test]
   public async Task execute_invokes_report_invocation_error()
   {
      var exception = new Exception();
      var result = new ExceptionResult(exception);

      var lambdaRuntime = A.Fake<ILambdaRuntime>();
      var resultExecutor = new ExceptionResult.Executor(lambdaRuntime);
      var serviceProvider = ServiceProviderFake.Returns<ILambdaResultExecutor<ExceptionResult>>(resultExecutor);
      var context = LambdaContextFake.With(awsRequestId: "ArbitraryRequestId");

      await result.ExecuteResultAsync(context, serviceProvider);

      A.CallTo(() => lambdaRuntime.ReplyWithInvocationFailureAsync(exception, context))
         .MustHaveHappenedOnceExactly();
   }
}
