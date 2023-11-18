using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class PipelineDelegateFake
{
   public static PipelineDelegate Valid()
   {
      return  (_, _, _, _) => Task.FromResult<ILambdaResult>(new StringResult("ValidResult"));
   }

   public static PipelineDelegate Callback(Action<Stream, ILambdaContext, IServiceProvider, CancellationToken> callback)
   {
      return (stream, context, serviceProvider, cancellationToken) =>
      {
         callback(stream, context, serviceProvider, cancellationToken);

         return Task.FromResult<ILambdaResult>(new StringResult("ValidResult"));
      };
   }

   public static PipelineDelegate Throws(Exception exception)
   {
      return  (_, _, _, _) => throw exception;
   }
}
