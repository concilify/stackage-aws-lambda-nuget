using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Executors;

namespace Stackage.Aws.Lambda
{
   public class LambdaPipelineBuilder : ILambdaPipelineBuilder
   {
      private readonly IList<Func<PipelineDelegate, PipelineDelegate>> _components =
         new List<Func<PipelineDelegate, PipelineDelegate>>();

      public ILambdaPipelineBuilder Use(Func<PipelineDelegate, PipelineDelegate> middleware)
      {
         _components.Add(middleware);

         return this;
      }

      public PipelineDelegate Build()
      {
         PipelineDelegate pipeline = (inputStream, context, requestServices) =>
         {
            var handlerExecutor = requestServices.GetService<ILambdaHandlerExecutor>();

            if (handlerExecutor == null)
            {
               throw new InvalidOperationException("No handler configured. Please specify a handler via ILambdaHostBuilder.UseHandler.");
            }

            return handlerExecutor.ExecuteAsync(inputStream, context);
         };

         foreach (var component in _components.Reverse())
         {
            pipeline = component(pipeline);
         }

         return pipeline;
      }
   }
}
