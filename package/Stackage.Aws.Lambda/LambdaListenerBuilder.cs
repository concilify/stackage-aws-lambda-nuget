using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Executors;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Stackage.Aws.Lambda;

public class LambdaListenerBuilder
{
   /// <summary>
   /// The Lambda container freezes the process at a point where an HTTP request is in progress.
   /// We need to make sure we don't timeout waiting for the next invocation.
   /// </summary>
   private static readonly TimeSpan RuntimeApiHttpTimeout = TimeSpan.FromHours(12);

   private readonly List<Action<IServiceCollection, IServiceProvider>> _configureServices = new();
   private readonly LambdaPipelineBuilder _pipelineBuilder = new();

   public LambdaListenerBuilder UseHandler<THandler>()
      where THandler : class, ILambdaHandler<Stream>
   {
      _configureServices.Add((services, _) =>
      {
         services.AddTransient<ILambdaHandler<Stream>, THandler>();
         services.AddTransient<ILambdaHandlerExecutor, StreamLambdaHandlerExecutor>();
      });

      return this;
   }

   public LambdaListenerBuilder UseHandler<THandler, TRequest>()
      where THandler : class, ILambdaHandler<TRequest>
   {
      _configureServices.Add((services, _) =>
      {
         services.AddTransient<ILambdaHandler<TRequest>, THandler>();
         services.AddTransient<ILambdaHandlerExecutor, LambdaHandlerExecutor<TRequest>>();
      });

      return this;
   }

   public LambdaListenerBuilder UseStartup<TStartup>()
      where TStartup : ILambdaStartup
   {
      _configureServices.Add((services, hostServiceProvider) =>
      {
         var startup = ActivatorUtilities.CreateInstance<TStartup>(hostServiceProvider);

         startup.ConfigureServices(services);
         startup.ConfigurePipeline(_pipelineBuilder);
      });

      return this;
   }

   public LambdaListenerBuilder UseSerializer<TSerializer>() where TSerializer : class, ILambdaSerializer
   {
      _configureServices.Add((services, _) =>
      {
         services.AddSingleton<ILambdaSerializer, TSerializer>();
      });

      return this;
   }

   public ILambdaListener Build()
   {
      // TODO: Catch initialisation errors and send to the runtime API

      var services = new ServiceCollection();
      var hostServiceProvider = new HostServiceProvider();

      services.AddSingleton(hostServiceProvider.HostEnvironment);
      services.AddSingleton(hostServiceProvider.Configuration);
      services.AddLogging(builder =>
      {
         builder.AddConfiguration(hostServiceProvider.Configuration.GetSection("Logging"));
         builder.ClearProviders();
         builder.AddJsonConsole();
      });

      foreach (var configureService in _configureServices)
      {
         configureService(services, hostServiceProvider);
      }

      var serviceProvider = services.BuildServiceProvider();

      var runtimeApiClient = CreateRuntimeApiClient(serviceProvider);
      var lambdaSerializer = serviceProvider.GetRequiredService<ILambdaSerializer>();
      var logger = serviceProvider.GetRequiredService<ILogger<LambdaListener>>();
      var l = serviceProvider.GetRequiredService<ILogger<LoggingHttpClientHandler>>();

      var x = l.IsEnabled(LogLevel.Information);

      return new LambdaListener(
         runtimeApiClient,
         serviceProvider,
         _pipelineBuilder.Build(),
         lambdaSerializer,
         logger);
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
}
