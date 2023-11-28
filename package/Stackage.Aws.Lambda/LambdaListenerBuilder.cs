﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Executors;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda;

public class LambdaListenerBuilder
{
   /// The Lambda container freezes the process at a point where an HTTP request is in progress.
   /// We need to make sure we don't timeout waiting for the next invocation.
   private static readonly TimeSpan RuntimeApiHttpTimeout = TimeSpan.FromHours(12);

   private readonly List<Action<IServiceCollection, IServiceProvider>> _configureServices = new();
   private readonly LambdaPipelineBuilder _pipelineBuilder = new();

   public LambdaListenerBuilder UseHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]THandler>()
      where THandler : class, ILambdaHandler<Stream>
   {
      _configureServices.Add((services, _) =>
      {
         services.AddTransient<ILambdaHandler<Stream>, THandler>();
         services.AddTransient<ILambdaHandlerExecutor, StreamLambdaHandlerExecutor>();
      });

      return this;
   }

   public LambdaListenerBuilder UseHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]THandler, TInput>()
      where THandler : class, ILambdaHandler<TInput>
   {
      _configureServices.Add((services, _) =>
      {
         services.AddTransient<ILambdaHandler<TInput>, THandler>();
         services.AddTransient<ILambdaHandlerExecutor, LambdaHandlerExecutor<TInput>>();
      });

      return this;
   }

   public LambdaListenerBuilder UseStartup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TStartup>()
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

   public LambdaListenerBuilder UseSerializer<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TSerializer>()
      where TSerializer : class, ILambdaSerializer
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

      services.AddTransient<ILambdaRuntime, LambdaRuntime>();
      services.AddTransient<IRuntimeApiClient>(CreateRuntimeApiClient);

      services.AddLogging(builder =>
      {
         builder.AddConfiguration(hostServiceProvider.Configuration.GetSection("Logging"));
         builder.ClearProviders();
         builder.AddJsonConsole();
      });

      services.AddTransient<ILambdaResultExecutor<StreamResult>, StreamResult.Executor>();
      services.AddTransient<ILambdaResultExecutor<StringResult>, StringResult.Executor>();
      services.AddTransient<ILambdaResultExecutor<ObjectResult>, ObjectResult.Executor>();
      services.AddTransient<ILambdaResultExecutor<CancellationResult>, CancellationResult.Executor>();
      services.AddTransient<ILambdaResultExecutor<ExceptionResult>, ExceptionResult.Executor>();

      foreach (var configureService in _configureServices)
      {
         configureService(services, hostServiceProvider);
      }

      var serviceProvider = services.BuildServiceProvider();

      var lambdaRuntime = serviceProvider.GetRequiredService<ILambdaRuntime>();

      return new LambdaListener(
         lambdaRuntime,
         serviceProvider,
         _pipelineBuilder.Build());
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
      var amazonLambdaRuntimeSupportVersion = typeof(LambdaBootstrap).Assembly.GetName().Version;
      var userAgentString = $"aws-lambda-dotnet/{dotnetRuntimeVersion}-{amazonLambdaRuntimeSupportVersion}";

      httpClient.DefaultRequestHeaders.Add("User-Agent", userAgentString);
      httpClient.Timeout = RuntimeApiHttpTimeout;
   }
}
