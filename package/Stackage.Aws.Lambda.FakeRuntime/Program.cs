using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Stackage.Core.Extensions;

namespace Stackage.Aws.Lambda.FakeRuntime
{
   public static class Program
   {
      public static async Task Main(string[] args)
      {
         var host = CreateHostBuilder(args)
            .Build();

         await host.RunAsync();
      }

      private static IHostBuilder CreateHostBuilder(string[] args)
      {
         return new HostBuilder()
            .UseDefaultBuilder(args)
            .UseSerilog((context, builder) => { builder.ReadFrom.Configuration(context.Configuration); })
            .ConfigureWebHost(webHostBuilder =>
            {
               webHostBuilder
                  .UseKestrel((context, options) =>
                  {
                     options.AddServerHeader = false;
                     options.Configure(context.Configuration.GetSection("Kestrel"));
                  })
                  .UseStartup<FakeRuntimeStartup>();
            });
      }
   }
}
