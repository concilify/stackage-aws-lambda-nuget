using Amazon.Lambda.Serialization.SystemTextJson;
using Lambda.Middleware.Example;
using Lambda.Middleware.Example.Handler;
using Lambda.Middleware.Example.Model;
using Stackage.Aws.Lambda;

using var consoleLifetime = new ConsoleLifetime();

await new LambdaListenerBuilder()
   .UseHandler<ObjectLambdaHandler, InputPoco>()
   .UseStartup<LambdaStartup>()
   .UseSerializer<SourceGeneratorLambdaJsonSerializer<MiddlewareExampleJsonSerializerContext>>()
   .Build()
   .ListenAsync(consoleLifetime.Token);
