using Lambda.Middleware.Example.Integrations;
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
         services.AddSingleton<ILambdaResultFactory, HttpLambdaResultFactory>();
         services.AddDeadlineCancellation();

         // TODO: Create correlationId type service
      }

      public void ConfigurePipeline(ILambdaPipelineBuilder pipelineBuilder)
      {
         pipelineBuilder.Use<RequestLoggingMiddleware>();
         pipelineBuilder.Use<ExceptionHandlingMiddleware>();
         pipelineBuilder.Use<DeadlineCancellationMiddleware>();
      }
   }
}
