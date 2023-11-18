using System.Threading;
using Amazon.Lambda.RuntimeSupport;
using FakeItEasy;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class RuntimeApiClientFake
{
   public static IRuntimeApiClient Valid()
   {
      var runtimeApiClient = A.Fake<IRuntimeApiClient>();

      A.CallTo(() => runtimeApiClient.GetNextInvocationAsync(A<CancellationToken>._))
         .Returns(A.Fake<InvocationRequest>());

      return runtimeApiClient;
   }
}
