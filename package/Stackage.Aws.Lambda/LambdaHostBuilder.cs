using System;
using System.IO;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class LambdaHostBuilder : LambdaHostBuilder<Stream>, ILambdaHostBuilder
   {
      public LambdaHostBuilder(IHostBuilder builder) : base(builder)
      {
      }
   }

   public class LambdaHostBuilder<TRequest> : ILambdaHostBuilder<TRequest>
   {
      private readonly IHostBuilder _builder;

      public LambdaHostBuilder(IHostBuilder builder)
      {
         _builder = builder;
      }

      public void ConfigureServices<TStartup>() where TStartup : IConfigureServices
      {
         _builder.ConfigureServices((context, services) =>
         {
            var startup = ActivatorUtilities.CreateInstance<TStartup>(new HostServiceProvider(context));

            startup.ConfigureServices(services);
         });
      }

      public void ConfigurePipeline<TStartup>() where TStartup : IConfigurePipeline<TRequest>
      {
         _builder.ConfigureServices((context, services) =>
         {
            var startup = ActivatorUtilities.CreateInstance<TStartup>(new HostServiceProvider(context));

            services.Configure<LambdaPipelineBuilderOptions<TRequest>>(options =>
            {
               options.ConfigurePipeline = app =>
               {
                  startup.Configure(app);
               };
            });
         });
      }

      public void UseSerializer<TSerializer>() where TSerializer : class, ILambdaSerializer
      {
         _builder.ConfigureServices(services =>
         {
            services.AddSingleton<ILambdaSerializer, TSerializer>();
         });
      }

      public void UseHandler(Func<IServiceProvider, PipelineDelegate<TRequest>> handlerFactory)
      {
         _builder.ConfigureServices(services =>
         {
            services.AddScoped(handlerFactory);
         });
      }
   }
}
