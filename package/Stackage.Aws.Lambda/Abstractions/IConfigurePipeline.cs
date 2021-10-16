namespace Stackage.Aws.Lambda.Abstractions
{
   public interface IConfigurePipeline<TRequest>
   {
      void ConfigurePipeline(ILambdaPipelineBuilder<TRequest> pipelineBuilder);
   }
}
