using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Middleware;

namespace Stackage.Aws.Lambda.Tests
{
   public class StartupWithDeadlineCancellation : ILambdaStartup
   {
      private readonly IConfiguration _configuration;

      public StartupWithDeadlineCancellation(IConfiguration configuration)
      {
         _configuration = configuration;
      }

      public void ConfigureServices(IServiceCollection services)
      {
         services.AddDeadlineCancellation(_configuration);
      }

      public void ConfigurePipeline(ILambdaPipelineBuilder pipelineBuilder)
      {
         pipelineBuilder.Use<DeadlineCancellationMiddleware>();
      }
   }
}
