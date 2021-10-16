using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Middleware.Example.Handler;
using Lambda.Middleware.Example.Model;
using Microsoft.Extensions.Hosting;
using Serilog;
using Stackage.Aws.Lambda;
using Stackage.Aws.Lambda.Extensions;

namespace Lambda.Middleware.Example
{
   public static class Program
   {
      public static async Task Main()
      {
         // TODO: How best to catch initialisation errors and send to the runtime API

         var host = LambdaHost.Create<APIGatewayHttpApiV2ProxyRequest<InputPoco>>(builder =>
            {
               builder.ConfigureServices<LambdaStartup>();
               builder.ConfigurePipeline<LambdaStartup>();
               builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
               builder.UseHandler<ObjectLambdaHandler, APIGatewayHttpApiV2ProxyRequest<InputPoco>>();
            })
            .UseSerilog((context, builder) =>
            {
               builder.ReadFrom.Configuration(context.Configuration);
            })
            .Build();

         await host.RunAsync();
      }
   }
}
