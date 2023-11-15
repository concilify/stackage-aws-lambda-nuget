using System.IO;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Executors;

namespace Stackage.Aws.Lambda
{
   public class LambdaHostBuilder : ILambdaHostBuilder
   {
      private readonly IHostBuilder _builder;

      public LambdaHostBuilder(IHostBuilder builder)
      {
         _builder = builder;
      }

      public void UseStartup<TStartup>() where TStartup : ILambdaStartup
      {
         _builder.ConfigureServices((context, services) =>
         {
            var startup = ActivatorUtilities.CreateInstance<TStartup>(new HostServiceProvider(context));

            startup.ConfigureServices(services);

            services.Configure<LambdaPipelineBuilderOptions>(options =>
            {
               options.ConfigurePipeline = app =>
               {
                  startup.ConfigurePipeline(app);
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

      public void UseHandler<THandler>()
         where THandler : class, ILambdaHandler<Stream>
      {
         _builder.ConfigureServices(services =>
         {
            services.AddTransient<ILambdaHandler<Stream>, THandler>();
            services.AddTransient<ILambdaHandlerExecutor, StreamLambdaHandlerExecutor>();
         });
      }

      public void UseHandler<THandler, TRequest>()
         where THandler : class, ILambdaHandler<TRequest>
      {
         _builder.ConfigureServices(services =>
         {
            services.AddTransient<ILambdaHandler<TRequest>, THandler>();
            services.AddTransient<ILambdaHandlerExecutor, LambdaHandlerExecutor<TRequest>>();
         });
      }
   }
}
