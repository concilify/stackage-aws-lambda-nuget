using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda;
using Stackage.Aws.Lambda.Extensions;

namespace Lambda.Basic.Example
{
   public static class Program
   {
      public static async Task Main()
      {
         // TODO: How best to catch initialisation errors and send to the runtime API

         var host = LambdaHost.Create(builder =>
            {
               builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
               builder.UseHandler<EchoLambdaHandler>();
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
