using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Middleware;

namespace Stackage.Aws.Lambda.Tests
{
   public class StartupWithDeadlineCancellation<TRequest> : ILambdaStartup
   {
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddDeadlineCancellation();
      }

      public void ConfigurePipeline(ILambdaPipelineBuilder pipelineBuilder)
      {
         pipelineBuilder.Use<DeadlineCancellationMiddleware<TRequest>, TRequest>();
      }
   }
}
