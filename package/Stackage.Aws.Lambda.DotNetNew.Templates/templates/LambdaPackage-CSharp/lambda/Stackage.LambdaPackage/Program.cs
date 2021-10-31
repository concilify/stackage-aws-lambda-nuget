using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda;
using Stackage.Aws.Lambda.Extensions;

namespace Stackage.LambdaPackage
{
   public static class Program
   {
      public static async Task Main()
      {
         var host = LambdaHost.Create<Request>(builder =>
            {
               builder.UseStartup<LambdaStartup>();
               builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
               builder.UseHandler<LambdaHandler, Request>();
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
