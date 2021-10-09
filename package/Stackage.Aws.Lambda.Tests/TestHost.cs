using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.FakeRuntime;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.Tests
{
   public static class TestHost
   {
      public static async Task<LambdaFunction.Dictionary> RunAsync(
         Action<ILambdaHostBuilder> configure,
         string functionName,
         params LambdaRequest[] invokeRequests)
      {
         return await RunAsync(LambdaHost.Create(builder =>
            {
               builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
               configure(builder);
            }),
            functionName,
            invokeRequests);
      }

      public static async Task<LambdaFunction.Dictionary> RunAsync<TRequest>(
         Action<ILambdaHostBuilder<TRequest>> configure,
         string functionName,
         params LambdaRequest[] invokeRequests)
      {
         return await RunAsync(LambdaHost.Create<TRequest>(builder =>
            {
               builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
               configure(builder);
            }),
            functionName,
            invokeRequests);
      }

      private static async Task<LambdaFunction.Dictionary> RunAsync(
         IHostBuilder builder,
         string functionName,
         params LambdaRequest[] invokeRequests)
      {
         Environment.SetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API", $"localhost:9001/{functionName}");

         var tokenSource = new CancellationTokenSource(5000);

         using var host = builder
            .ConfigureAppConfiguration(configurationBuilder =>
            {
               configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
               {
                  {"KESTREL:ENDPOINTS:HTTP:URL", "http://localhost:9001"}
               });
            })
            .ConfigureServices(services =>
            {
               services.Configure<HostOptions>(options =>
               {
                  options.ShutdownTimeout = TimeSpan.FromMilliseconds(10);
               });
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

         var function = new LambdaFunction(functionName);

         foreach (var request in invokeRequests)
         {
            function.Requests.Enqueue(request);
         }

         var functions = host.Services.GetRequiredService<LambdaFunction.Dictionary>();
         functions.TryAdd(function.Name, function);

         await host.RunAsync(tokenSource.Token);

         return functions;
      }
   }
}
