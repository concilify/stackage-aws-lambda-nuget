using Lambda.Basic.Example;
using Stackage.Aws.Lambda;

using var consoleLifetime = new ConsoleLifetime();

await new LambdaListenerBuilder()
   .UseHandler<EchoLambdaHandler>()
   .Build()
   .ListenAsync(consoleLifetime.Token);
