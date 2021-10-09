using Lambda.Middleware.Example.Middleware;
using Lambda.Middleware.Example.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Extensions;

namespace Lambda.Middleware.Example
{
   public class LambdaStartup : IConfigureServices, IConfigurePipeline<InputPoco>
   {
      private readonly IConfiguration _configuration;

      public LambdaStartup(IConfiguration configuration)
      {
         _configuration = configuration;
      }

      public void ConfigureServices(IServiceCollection services)
      {
         // TODO: Create correlationId type service
      }

      public void Configure(ILambdaPipelineBuilder<InputPoco> pipelineBuilder)
      {
         pipelineBuilder.Use<RequestLoggingMiddleware<InputPoco>, InputPoco>();
         pipelineBuilder.Use<ExceptionHandlingMiddleware<InputPoco>, InputPoco>();
      }
   }
}
