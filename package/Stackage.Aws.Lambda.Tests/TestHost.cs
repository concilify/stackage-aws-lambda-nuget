using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
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
         Action<ILambdaHostBuilder> configure,
         string functionName,
         params LambdaRequest[] invokeRequests)
      {
         return await RunAsync<Stream>(LambdaHost.Create(builder =>
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
         return await RunAsync<TRequest>(LambdaHost.Create<TRequest>(builder =>
            {
               builder.UseSerializer<CamelCaseLambdaJsonSerializer>();
               configure(builder);
            }),
            functionName,
            invokeRequests);
      }

      private static async Task<LambdaFunction.Dictionary> RunAsync<TRequest>(
         IHostBuilder builder,
         string functionName,
         params LambdaRequest[] invokeRequests)
      {
         Environment.SetEnvironmentVariable("AWS_LAMBDA_RUNTIME_API", $"{RuntimeApiHostAndPort}/{functionName}");

         var tokenSource = new CancellationTokenSource(5000);

         using var host = builder
            .ConfigureAppConfiguration(configurationBuilder =>
            {
               configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
               {
                  {"KESTREL:ENDPOINTS:HTTP:URL", $"http://{RuntimeApiHostAndPort}"}
               });
            })
            .UseSerilog((_, configuration) =>
            {
               configuration.MinimumLevel.Debug();
               configuration.WriteTo.Console();
            })
            .ConfigureServices(services =>
            {
               services.Decorate<ILambdaListener<TRequest>, DelayedStartLambdaListener<TRequest>>();
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

      // Delay listener start to allow for fake runtime api to start
      private class DelayedStartLambdaListener<TRequest> : ILambdaListener<TRequest>
      {
         private readonly ILambdaListener<TRequest> _innerListener;

         public DelayedStartLambdaListener(ILambdaListener<TRequest> innerListener)
         {
            _innerListener = innerListener;
         }

         public async Task ListenAsync(CancellationToken cancellationToken)
         {
            using var httpClient = new HttpClient { BaseAddress = new Uri($"http://{RuntimeApiHostAndPort}")};

            for (var i = 0; i < 30; i++)
            {
               try
               {
                  var response = await httpClient.GetAsync("health", cancellationToken);

                  if (response.StatusCode == HttpStatusCode.OK)
                  {
                     break;
                  }
               }
               catch
               {
                  await Task.Delay(100, cancellationToken);
               }
            }

            await _innerListener.ListenAsync(cancellationToken);
         }
      }
   }
}
