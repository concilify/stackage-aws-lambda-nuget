using System;
using System.IO;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaHostBuilder : ILambdaHostBuilder<Stream>
   {
   }

   public interface ILambdaHostBuilder<TRequest>
   {
      void ConfigureServices<TStartup>() where TStartup : IConfigureServices;

      void ConfigurePipeline<TStartup>() where TStartup : IConfigurePipeline<TRequest>;

      void UseSerializer<TSerializer>() where TSerializer : class, ILambdaSerializer;

      void UseHandler(Func<IServiceProvider, PipelineDelegate<TRequest>> handlerFactory);
   }
}
