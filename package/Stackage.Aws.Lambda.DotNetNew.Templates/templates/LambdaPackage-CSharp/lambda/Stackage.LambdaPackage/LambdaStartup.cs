using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Middleware;

namespace Stackage.LambdaPackage
{
   // TODO: Register any services and/or middleware or remove
   public class LambdaStartup : ILambdaStartup<Request>
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
      }

      public void ConfigurePipeline(ILambdaPipelineBuilder<Request> pipelineBuilder)
      {
         pipelineBuilder.Use<ExceptionHandlingMiddleware<Request>, Request>();
         pipelineBuilder.Use<DeadlineCancellationMiddleware<Request>, Request>();
      }
   }
}
