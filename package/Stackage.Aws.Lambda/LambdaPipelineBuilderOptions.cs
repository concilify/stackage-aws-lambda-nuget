using System;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class LambdaPipelineBuilderOptions<TRequest>
   {
      public Action<ILambdaPipelineBuilder<TRequest>>? ConfigurePipeline { get; set; }
   }
}
