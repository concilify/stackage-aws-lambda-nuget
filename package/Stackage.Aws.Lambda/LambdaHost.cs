using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda
{
   public static class LambdaHost
   {
      /// <summary>
      /// The Lambda container freezes the process at a point where an HTTP request is in progress.
      /// We need to make sure we don't timeout waiting for the next invocation.
      /// </summary>
      private static readonly TimeSpan RuntimeApiHttpTimeout = TimeSpan.FromHours(12);

      public static IHostBuilder Create(Action<ILambdaHostBuilder> configure)
      {
         return Create()
            .ConfigureListener<Stream, StreamParser>()
            .ConfigureLambdaHost(configure);
      }

      public static IHostBuilder Create<TRequest>(Action<ILambdaHostBuilder<TRequest>> configure)
      {
         return Create()
            .ConfigureListener<TRequest, RequestParser<TRequest>>()
            .ConfigureLambdaHost(configure);
      }

      private static IHostBuilder ConfigureListener<TRequest, TParser>(this IHostBuilder hostBuilder)
         where TParser : class, IRequestParser<TRequest>
      {
         return hostBuilder
            .ConfigureServices(services =>
            {
               services.AddHostedService<LambdaListenerHostedService<TRequest>>();
               services.AddSingleton<ILambdaListener<TRequest>, LambdaListener<TRequest>>();
               services.AddSingleton<ILambdaPipelineBuilder<TRequest>, LambdaPipelineBuilder<TRequest>>();
               services.AddSingleton<IRuntimeApiClient>(CreateRuntimeApiClient);
               services.AddSingleton<IRequestHandler<TRequest>, RequestHandler<TRequest>>();
               services.AddSingleton<IRequestParser<TRequest>, TParser>();
               services.AddSingleton<ILambdaResultFactory, DefaultLambdaResultFactory>();

               // TODO: Lambda fails after 3 seconds when using this
               // services.AddHttpClient<RuntimeApiClient>(ConfigureRuntimeHttpClient);
            });
      }

      private static RuntimeApiClient CreateRuntimeApiClient(IServiceProvider serviceProvider)
      {
         var logger = serviceProvider.GetRequiredService<ILogger<LoggingHttpClientHandler>>();

         var httpClient = new HttpClient(new LoggingHttpClientHandler(new HttpClientHandler(), logger));

         ConfigureRuntimeHttpClient(httpClient);

         return new RuntimeApiClient(httpClient);
      }

      private static void ConfigureRuntimeHttpClient(HttpClient httpClient)
      {
         var dotnetRuntimeVersion = new DirectoryInfo(RuntimeEnvironment.GetRuntimeDirectory()).Name;
         var amazonLambdaRuntimeSupport = typeof(LambdaBootstrap).Assembly.GetName().Version;
         var userAgentString = $"aws-lambda-dotnet/{dotnetRuntimeVersion}-{amazonLambdaRuntimeSupport}";

         httpClient.DefaultRequestHeaders.Add("User-Agent", userAgentString);
         httpClient.Timeout = RuntimeApiHttpTimeout;
      }

      private static IHostBuilder Create()
      {
         return new HostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureHostConfiguration(builder => builder.AddEnvironmentVariables(prefix: "DOTNET_"))
            .ConfigureAppConfiguration((context, builder) =>
            {
               var env = context.HostingEnvironment;

               builder.AddJsonFile("appsettings.json", optional: true)
                  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

               builder.AddEnvironmentVariables();
            })
            .UseDefaultServiceProvider((context, options) =>
            {
               var isDevelopment = context.HostingEnvironment.IsDevelopment();

               options.ValidateScopes = isDevelopment;
               options.ValidateOnBuild = isDevelopment;
            })
            .ConfigureServices(services =>
            {
               services.Configure<ConsoleLifetimeOptions>(options =>
               {
                  // TODO: Investigate why these (when enabled) appear after the request during cold-start
                  options.SuppressStatusMessages = true;
               });
            });
      }
   }
}
