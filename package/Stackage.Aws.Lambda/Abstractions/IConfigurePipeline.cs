namespace Stackage.Aws.Lambda.Abstractions
{
   public interface IConfigurePipeline<TRequest>
   {
      void Configure(ILambdaPipelineBuilder<TRequest> pipelineBuilder);
   }
}
