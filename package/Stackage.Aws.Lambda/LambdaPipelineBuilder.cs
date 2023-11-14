using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

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
         PipelineDelegate<TRequest> pipeline = (request, context, requestServices) =>
         {
            var handlerAsync = requestServices.GetService<PipelineDelegate>();

            if (handlerAsync == null)
            {
               throw new InvalidOperationException("No handler configured. Please specify a handler via ILambdaHostBuilder.UseHandler.");
            }

            return handlerAsync(request, context, requestServices);
         };

         foreach (var component in _components.Reverse())
         {
            pipeline = component(pipeline);
         }

         return pipeline;
      }
   }
}
