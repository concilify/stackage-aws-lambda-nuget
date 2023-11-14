using System;
using System.IO;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaHostBuilder
   {
      void UseStartup<TStartup>() where TStartup : ILambdaStartup;

      void UseSerializer<TSerializer>() where TSerializer : class, ILambdaSerializer;

      void UseHandler(Func<IServiceProvider, PipelineDelegate> handlerFactory);
   }
}
