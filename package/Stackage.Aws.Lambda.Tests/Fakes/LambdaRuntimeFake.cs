using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class LambdaRuntimeFake
{
   public static ILambdaRuntime Valid() => new WaitForInvocationLambdaRuntime(_ => A.Fake<ILambdaInvocation>());

   public static ILambdaRuntime WaitForInvocationCallback(Func<CancellationToken, ILambdaInvocation> callback)
      => new WaitForInvocationLambdaRuntime(callback);

   public static ILambdaRuntime WaitForInvocationCallback(Action<CancellationToken> callback)
      => new WaitForInvocationLambdaRuntime(token =>
      {
         callback(token);
         return A.Fake<ILambdaInvocation>();
      });

   public static ILambdaRuntime ReplyWithInvocationSuccessCallback(Action<Stream, ILambdaContext> callback)
   {
      var runtimeApiClient = A.Fake<ILambdaRuntime>();

      A.CallTo(() => runtimeApiClient.ReplyWithInvocationSuccessAsync(A<Stream>._, A<ILambdaContext>._))
         .Invokes(callback);

      return runtimeApiClient;
   }

   public static ILambdaRuntime ReplyWithInvocationFailureCallback(Action<Exception, ILambdaContext> callback)
   {
      var runtimeApiClient = A.Fake<ILambdaRuntime>();

      A.CallTo(() => runtimeApiClient.ReplyWithInvocationFailureAsync(A<Exception>._, A<ILambdaContext>._))
         .Invokes(callback);

      return runtimeApiClient;
   }

   private class WaitForInvocationLambdaRuntime : ILambdaRuntime
   {
      private readonly Func<CancellationToken, ILambdaInvocation> _callback;

      public WaitForInvocationLambdaRuntime(Func<CancellationToken, ILambdaInvocation> callback)
      {
         _callback = callback ?? throw new ArgumentNullException(nameof(callback));
      }

      public Task<ILambdaInvocation> WaitForInvocationAsync(CancellationToken cancellationToken)
      {
         return Task.FromResult(_callback(cancellationToken));
      }

      public Task ReplyWithInvocationSuccessAsync(Stream outputStream, ILambdaContext context)
      {
         throw new NotSupportedException();
      }

      public Task ReplyWithInvocationFailureAsync(Exception exception, ILambdaContext context)
      {
         throw new NotSupportedException();
      }
   }
}
