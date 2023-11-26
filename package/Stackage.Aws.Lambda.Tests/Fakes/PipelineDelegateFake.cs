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
      return async (_, _, _, requestAborted) =>
      {
         if (latencyMs != null)
         {
            await Task.Delay(latencyMs.Value, requestAborted);
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

   public static PipelineDelegate ThrowsAfterCancellation(Exception exception)
   {
      return async (_, _, _, requestAborted) =>
      {
         try
         {
            await Task.Delay(10000, requestAborted);
         }
         catch (Exception)
         {
            throw exception;
         }

         throw new NotSupportedException("Shouldn't get here");
      };
   }

   public static PipelineDelegate Callback(Action<Stream, ILambdaContext, IServiceProvider, CancellationToken> callback)
   {
      return (stream, context, serviceProvider, requestAborted) =>
      {
         callback(stream, context, serviceProvider, requestAborted);

         return Task.FromResult<ILambdaResult>(new StringResult("ValidResult"));
      };
   }

   public static PipelineDelegate LongRunningAndExpectsToBeCancelled()
   {
      return AsyncCallback(
         async (_, _, _, requestAborted) =>
         {
            Assert.That(requestAborted.IsCancellationRequested, Is.False);

            try
            {
               await Task.Delay(10000, requestAborted);
            }
            catch (Exception e)
            {
               Assert.That(requestAborted.IsCancellationRequested, Is.True);
               throw;
            }

            throw new NotSupportedException("Shouldn't get here");
         });
   }

   private static PipelineDelegate AsyncCallback(Func<Stream, ILambdaContext, IServiceProvider, CancellationToken, Task> callback)
   {
      return async (stream, context, serviceProvider, requestAborted) =>
      {
         await callback(stream, context, serviceProvider, requestAborted);

         return new StringResult("ValidResult");
      };
   }
}
