using Lambda.Middleware.Example.Middleware;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Middleware;

namespace Lambda.Middleware.Example
{
   public class LambdaStartup : ILambdaStartup
   {
      private readonly IConfiguration _configuration;

      public LambdaStartup(IConfiguration configuration)
      {
         _configuration = configuration;
      }

      public void ConfigureServices(IServiceCollection services)
      {
         services.AddDeadlineCancellation(_configuration);

         // TODO: Create correlationId type service
      }

      public void ConfigurePipeline(ILambdaPipelineBuilder pipelineBuilder)
      {
         // Middleware runs in the order listed here
         pipelineBuilder.Use<RequestLoggingMiddleware>();
         pipelineBuilder.Use<DeadlineCancellationMiddleware>();
      }
   }
}
