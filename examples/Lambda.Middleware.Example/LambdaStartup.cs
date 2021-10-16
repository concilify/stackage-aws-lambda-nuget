using Amazon.Lambda.APIGatewayEvents;
using Lambda.Middleware.Example.Integrations;
using Lambda.Middleware.Example.Middleware;
using Lambda.Middleware.Example.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Middleware;

namespace Lambda.Middleware.Example
{
   public class LambdaStartup : IConfigureServices, IConfigurePipeline<APIGatewayHttpApiV2ProxyRequest<InputPoco>>
   {
      private readonly IConfiguration _configuration;

      public LambdaStartup(IConfiguration configuration)
      {
         _configuration = configuration;
      }

      public void ConfigureServices(IServiceCollection services)
      {
         services.AddSingleton<ILambdaResultFactory, HttpApiV2LambdaResultFactory>();

         // TODO: Create correlationId type service
      }

      public void Configure(ILambdaPipelineBuilder<APIGatewayHttpApiV2ProxyRequest<InputPoco>> pipelineBuilder)
      {
         pipelineBuilder.Use<RequestLoggingMiddleware<APIGatewayHttpApiV2ProxyRequest<InputPoco>>, APIGatewayHttpApiV2ProxyRequest<InputPoco>>();
         pipelineBuilder.Use<ExceptionHandlingMiddleware<APIGatewayHttpApiV2ProxyRequest<InputPoco>>, APIGatewayHttpApiV2ProxyRequest<InputPoco>>();
      }
   }
}
