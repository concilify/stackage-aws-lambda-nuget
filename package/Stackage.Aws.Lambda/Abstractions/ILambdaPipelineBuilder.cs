using System;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaPipelineBuilder<TRequest>
   {
      ILambdaPipelineBuilder<TRequest> Use(Func<PipelineDelegate<TRequest>, PipelineDelegate<TRequest>> middleware);

      PipelineDelegate<TRequest> Build();
   }
}
