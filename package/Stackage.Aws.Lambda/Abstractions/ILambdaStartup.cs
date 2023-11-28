using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaStartup
   {
      void ConfigureServices(IServiceCollection services);

      void ConfigurePipeline(ILambdaPipelineBuilder pipelineBuilder);
   }
}
