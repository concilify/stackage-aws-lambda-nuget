using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using FakeItEasy;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class ObjectResultExecutorTests
{
   [Test]
   public async Task execute_invokes_send_response()
   {
      string streamAsString = null;

      var @object = new StringPoco { Value = "ArbitraryString" };
      var result = new ObjectResult(@object);

      var lambdaSerializer = new CamelCaseLambdaJsonSerializer();
      var lambdaRuntime = LambdaRuntimeFake.ReplyWithInvocationSuccessCallback((stream, _) => streamAsString = stream.ReadToEnd());
      var resultExecutor = new ObjectResult.Executor(lambdaSerializer, lambdaRuntime);
      var serviceProvider = ServiceProviderFake.Returns<ILambdaResultExecutor<ObjectResult>>(resultExecutor);
      var context = LambdaContextFake.With(awsRequestId: "ArbitraryRequestId");

      await result.ExecuteResultAsync(context, serviceProvider);

      A.CallTo(() => lambdaRuntime.ReplyWithInvocationSuccessAsync(A<Stream>._, context))
         .MustHaveHappenedOnceExactly();

      Assert.That(streamAsString, Is.EqualTo("{\"value\":\"ArbitraryString\"}"));
   }
}
