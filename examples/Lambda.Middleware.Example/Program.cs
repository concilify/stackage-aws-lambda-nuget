using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Middleware.Example;
using Lambda.Middleware.Example.Handler;
using Lambda.Middleware.Example.Model;
using Stackage.Aws.Lambda;

//namespace Lambda.Middleware.Example;
// {
//    public static class Program
//    {
//       public static async Task Main()
//       {
//          // TODO: How best to catch initialisation errors and send to the runtime API
//
//          var host = LambdaHost.Create(builder =>
//             {
//                builder.UseStartup<LambdaStartup>();
//                builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
//                builder.UseHandler<ObjectLambdaHandler, InputPoco>();
//             })
//             .ConfigureLogging(builder =>
//             {
//                builder.AddJsonConsole();
//             })
//             .Build();
//
//          await host.RunAsync();
//       }
//    }
// }

using var consoleLifetime = new ConsoleLifetime();

await new LambdaListenerBuilder()
   .UseHandler<ObjectLambdaHandler, InputPoco>()
   .UseStartup<LambdaStartup>()
   .UseSerializer<CamelCaseLambdaJsonSerializer>()
   .Build()
   .ListenAsync(consoleLifetime.Token);
