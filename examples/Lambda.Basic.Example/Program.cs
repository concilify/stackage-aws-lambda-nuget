using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda;

namespace Lambda.Basic.Example
{
   public static class Program
   {
      public static async Task Main()
      {
         // TODO: How best to catch initialisation errors and send to the runtime API

         var host = LambdaHost.Create(builder =>
            {
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
