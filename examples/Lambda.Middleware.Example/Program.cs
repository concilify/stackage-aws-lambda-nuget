using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Middleware.Example.Handler;
using Lambda.Middleware.Example.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda;
using Stackage.Aws.Lambda.Extensions;

namespace Lambda.Middleware.Example
{
   public static class Program
   {
      public static async Task Main()
      {
         // TODO: How best to catch initialisation errors and send to the runtime API

         var host = LambdaHost.Create<InputPoco>(builder =>
            {
               builder.UseStartup<LambdaStartup>();
               builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
               builder.UseHandler<ObjectLambdaHandler, InputPoco>();
            })
            .ConfigureLogging(builder =>
            {
               builder.AddJsonConsole();
            })
            .Build();

         await host.RunAsync();
      }
   }
}
