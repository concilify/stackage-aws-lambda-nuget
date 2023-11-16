using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.FakeRuntime;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.Tests
{
   public static class TestHost
   {
      private const string RuntimeApiHostAndPort = "localhost:9001";

      public static async Task<LambdaFunction.Dictionary> RunAsync(
         string functionName,
         LambdaRequest invokeRequest,
         Action<LambdaListenerBuilder> configureLambdaListener = null,
         Action<IConfigurationBuilder> configureConfiguration = null)
      {
         return await RunAsync(functionName, new[] { invokeRequest }, configureLambdaListener, configureConfiguration);
      }

      public static async Task<LambdaFunction.Dictionary> RunAsync(
         string functionName,
         IEnumerable<LambdaRequest> invokeRequests,
         Action<LambdaListenerBuilder> configureLambdaListener = null,
         Action<IConfigurationBuilder> configureConfiguration = null)
      {
         Environment.SetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API", $"{RuntimeApiHostAndPort}/{functionName}");

         var tokenSource = new CancellationTokenSource(5000);

         using var host = CreateHost(configureConfiguration, tokenSource);

         var lambdaListener = CreateLambdaListener(configureLambdaListener);

         var functions = host.Services.GetRequiredService<LambdaFunction.Dictionary>();

         QueueRequests(functions, functionName, invokeRequests);

         var hostRunTask = host.RunAsync(tokenSource.Token);
         var lambdaListenerListenTask = lambdaListener.ListenAsync(tokenSource.Token);

         await Task.WhenAll(hostRunTask, lambdaListenerListenTask);

         return functions;
      }

      private static IHost CreateHost(
         Action<IConfigurationBuilder> configureConfiguration,
         CancellationTokenSource tokenSource)
      {
         return new HostBuilder()
            .ConfigureAppConfiguration(configurationBuilder =>
            {
               configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
               {
                  {"KESTREL:ENDPOINTS:HTTP:URL", $"http://{RuntimeApiHostAndPort}"}
               });
               configureConfiguration?.Invoke(configurationBuilder);
            })
            .UseSerilog((_, configuration) =>
            {
               configuration.MinimumLevel.Debug();
               configuration.WriteTo.Console();
            })
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
               webHostBuilder.UseStartup<FakeRuntimeStartup>();
               webHostBuilder.ConfigureServices(services =>
               {
                  services.Decorate<IFunctionsService>(functionsService => new CancelOnResponseOrErrorFunctionsService(functionsService, tokenSource));
               });
            })
            .Build();
      }

      private static ILambdaListener CreateLambdaListener(Action<LambdaListenerBuilder> configureLambdaListener)
      {
         var lambdaListenerBuilder = new LambdaListenerBuilder()
            .UseSerializer<CamelCaseLambdaJsonSerializer>();

         configureLambdaListener(lambdaListenerBuilder);

         return lambdaListenerBuilder.Build();
      }

      private static void QueueRequests(
         LambdaFunction.Dictionary functions,
         string functionName,
         IEnumerable<LambdaRequest> invokeRequests)
      {
         var function = new LambdaFunction(functionName);

         foreach (var request in invokeRequests)
         {
            function.QueuedRequests.Enqueue(request);
         }

         functions.TryAdd(function.Name, function);
      }
   }
}
