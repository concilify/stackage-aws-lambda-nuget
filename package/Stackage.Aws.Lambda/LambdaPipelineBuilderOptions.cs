using System;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class LambdaPipelineBuilderOptions
   {
      public Action<ILambdaPipelineBuilder>? ConfigurePipeline { get; set; }
   }
}
