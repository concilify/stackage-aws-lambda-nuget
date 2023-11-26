using System.IO;
using System.Threading.Tasks;
using FakeItEasy;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class StringResultExecutorTests
{
   [Test]
   public async Task execute_invokes_send_response()
   {
      string streamAsString = null;

      const string @string = "ArbitraryString";

      var result = new StringResult(@string);

      var lambdaRuntime = LambdaRuntimeFake.ReplyWithInvocationSuccessCallback((stream, _) => streamAsString = stream.ReadToEnd());
      var resultExecutor = new StringResult.Executor(lambdaRuntime);
      var serviceProvider = ServiceProviderFake.Returns<ILambdaResultExecutor<StringResult>>(resultExecutor);
      var context = LambdaContextFake.With(awsRequestId: "ArbitraryRequestId");

      await result.ExecuteResultAsync(context, serviceProvider);

      A.CallTo(() => lambdaRuntime.ReplyWithInvocationSuccessAsync(A<Stream>._, context))
         .MustHaveHappenedOnceExactly();

      Assert.That(streamAsString, Is.EqualTo(@string));
   }
}
