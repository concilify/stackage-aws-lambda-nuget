using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class PipelineDelegateFake
{
   public static PipelineDelegate Valid() => Returns(new StringResult("ValidResult"));

   public static PipelineDelegate Returns(ILambdaResult lambdaResult, int? latencyMs = null)
   {
      return async (_, _, _, cancellationToken) =>
      {
         if (latencyMs != null)
         {
            await Task.Delay(latencyMs.Value, cancellationToken);
         }

         return lambdaResult;
      };
   }

   public static PipelineDelegate LongRunningIgnoresCancellationToken()
   {
      return async (_, _, _, _) =>
      {
         await Task.Delay(10000, CancellationToken.None);

         return new StringResult("ValidResult");
      };
   }

   public static PipelineDelegate Throws(Exception exception = null)
   {
      return  (_, _, _, _) => throw exception ?? new Exception();
   }

   public static PipelineDelegate Callback(Action<Stream, ILambdaContext, IServiceProvider, CancellationToken> callback)
   {
      return (stream, context, serviceProvider, cancellationToken) =>
      {
         callback(stream, context, serviceProvider, cancellationToken);

         return Task.FromResult<ILambdaResult>(new StringResult("ValidResult"));
      };
   }

   public static PipelineDelegate Callback(Func<Stream, ILambdaContext, IServiceProvider, CancellationToken, ILambdaResult> callback)
   {
      return (stream, context, serviceProvider, cancellationToken) =>
      {
         var result = callback(stream, context, serviceProvider, cancellationToken);

         return Task.FromResult(result);
      };
   }

   public static PipelineDelegate AsyncCallback(Func<Stream, ILambdaContext, IServiceProvider, CancellationToken, Task> callback)
   {
      return async (stream, context, serviceProvider, cancellationToken) =>
      {
         await callback(stream, context, serviceProvider, cancellationToken);

         return new StringResult("ValidResult");
      };
   }

   public static PipelineDelegate LongRunningAndExpectsToBeCancelled()
   {
      return AsyncCallback(
         async (_, _, _, cancellationToken) =>
         {
            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            await Task.Delay(10000, cancellationToken);

            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
         });
   }

   public static PipelineDelegate LongRunningAndDoesNotExpectToBeCancelled()
   {
      return AsyncCallback(
         async (_, _, _, cancellationToken) =>
         {
            Assert.That(cancellationToken.IsCancellationRequested, Is.False);

            await Task.Delay(10000, cancellationToken);

            Assert.That(cancellationToken.IsCancellationRequested, Is.False);
         });
   }
}
