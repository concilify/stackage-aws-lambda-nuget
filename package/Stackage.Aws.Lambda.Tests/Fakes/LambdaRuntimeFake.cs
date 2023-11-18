using System;
using System.IO;
using System.Threading;
using Amazon.Lambda.Core;
using FakeItEasy;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class LambdaRuntimeFake
{
   public static ILambdaRuntime Valid()
   {
      var runtimeApiClient = A.Fake<ILambdaRuntime>();

      A.CallTo(() => runtimeApiClient.WaitForInvocationAsync(A<CancellationToken>._))
         .Returns(A.Fake<ILambdaInvocation>());

      return runtimeApiClient;
   }

   public static ILambdaRuntime WaitForInvocationCallback(Action<CancellationToken> callback)
   {
      var runtimeApiClient = A.Fake<ILambdaRuntime>();

      A.CallTo(() => runtimeApiClient.WaitForInvocationAsync(A<CancellationToken>._))
         .Invokes(callback)
         .Returns(A.Fake<ILambdaInvocation>());

      return runtimeApiClient;
   }

   public static ILambdaRuntime ReplyWithInvocationSuccessCallback(Action<Stream, ILambdaContext, CancellationToken> callback)
   {
      var runtimeApiClient = A.Fake<ILambdaRuntime>();

      A.CallTo(() => runtimeApiClient.ReplyWithInvocationSuccessAsync(A<Stream>._, A<ILambdaContext>._,  A<CancellationToken>._))
         .Invokes(callback);

      return runtimeApiClient;
   }

   public static ILambdaRuntime ReplyWithInvocationFailureCallback(Action<Exception, ILambdaContext, CancellationToken> callback)
   {
      var runtimeApiClient = A.Fake<ILambdaRuntime>();

      A.CallTo(() => runtimeApiClient.ReplyWithInvocationFailureAsync(A<Exception>._, A<ILambdaContext>._,  A<CancellationToken>._))
         .Invokes(callback);

      return runtimeApiClient;
   }
}
