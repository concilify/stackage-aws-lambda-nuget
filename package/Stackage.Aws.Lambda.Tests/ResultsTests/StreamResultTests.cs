using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class StreamResultExecutorTests
{
   [Test]
   public async Task execute_invokes_send_response()
   {
      var stream = new MemoryStream();
      var result = new StreamResult(stream);

      var lambdaRuntime = A.Fake<ILambdaRuntime>();
      var resultExecutor = new StreamResult.Executor(lambdaRuntime);
      var serviceProvider = ServiceProviderFake.Returns<ILambdaResultExecutor<StreamResult>>(resultExecutor);
      var context = LambdaContextFake.With(awsRequestId: "ArbitraryRequestId");

      await result.ExecuteResultAsync(context, serviceProvider);

      A.CallTo(() => lambdaRuntime.ReplyWithInvocationSuccessAsync(stream, context))
         .MustHaveHappenedOnceExactly();
   }
}
