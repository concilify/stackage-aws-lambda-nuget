using System;
using System.IO;
using System.Threading;
using Amazon.Lambda.Core;
using FakeItEasy;
using Stackage.Aws.Lambda.Executors;

namespace Stackage.Aws.Lambda.Tests.Fakes;

internal static class LambdaHandlerExecutorFake
{
   public static ILambdaHandlerExecutor ExecuteCallback(Action<Stream, ILambdaContext, CancellationToken> callback)
   {
      var handlerExecutor = A.Fake<ILambdaHandlerExecutor>();

      A.CallTo(() => handlerExecutor.ExecuteAsync(A<Stream>._, A<ILambdaContext>._,  A<CancellationToken>._))
         .Invokes(callback);

      return handlerExecutor;
   }
}
