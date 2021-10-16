using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaStartup<TRequest>
   {
      void ConfigureServices(IServiceCollection services);

      void ConfigurePipeline(ILambdaPipelineBuilder<TRequest> pipelineBuilder);
   }
}
