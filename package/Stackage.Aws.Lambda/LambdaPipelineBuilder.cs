using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class LambdaPipelineBuilder<TRequest> : ILambdaPipelineBuilder<TRequest>
   {
      private readonly IList<Func<PipelineDelegate<TRequest>, PipelineDelegate<TRequest>>> _components =
         new List<Func<PipelineDelegate<TRequest>, PipelineDelegate<TRequest>>>();

      public ILambdaPipelineBuilder<TRequest> Use(Func<PipelineDelegate<TRequest>, PipelineDelegate<TRequest>> middleware)
      {
         _components.Add(middleware);

         return this;
      }

      public PipelineDelegate<TRequest> Build()
      {
         PipelineDelegate<TRequest> pipeline = (request, context, requestServices) =>
         {
            var handlerAsync = requestServices.GetService<PipelineDelegate<TRequest>>();

            if (handlerAsync == null)
            {
               throw new InvalidOperationException($"No handler configured. Please specify a handler via ILambdaHostBuilder.UseHandler.");
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
