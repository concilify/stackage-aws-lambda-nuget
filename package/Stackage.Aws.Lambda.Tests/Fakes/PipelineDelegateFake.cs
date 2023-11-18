using System;
using System.Threading.Tasks;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class PipelineDelegateFake
{
   public static PipelineDelegate Valid()
   {
      return  (_, _, _, _) => Task.FromResult<ILambdaResult>(new StringResult("ValidResult"));
   }

   public static PipelineDelegate Throws(Exception exception)
   {
      return  (_, _, _, _) => throw exception;
   }
}
