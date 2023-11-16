using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Basic.Example;
using Stackage.Aws.Lambda;

using var consoleLifetime = new ConsoleLifetime();

await new LambdaListenerBuilder()
   .UseHandler<EchoLambdaHandler>()
   .UseSerializer<CamelCaseLambdaJsonSerializer>()
   .Build()
   .ListenAsync(consoleLifetime.Token);
