using System;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaPipelineBuilder
   {
      ILambdaPipelineBuilder Use(Func<PipelineDelegate, PipelineDelegate> middleware);
   }
}
