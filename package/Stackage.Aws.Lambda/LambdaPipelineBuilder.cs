using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Executors;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Middleware;

namespace Stackage.Aws.Lambda
{
   public class LambdaPipelineBuilder : ILambdaPipelineBuilder
   {
      private readonly List<Func<PipelineDelegate, PipelineDelegate>> _middlewares = new();

      public ILambdaPipelineBuilder Use(Func<PipelineDelegate, PipelineDelegate> middleware)
      {
         _middlewares.Add(middleware);

         return this;
      }

      public PipelineDelegate Build()
      {
         PipelineDelegate pipeline = (inputStream, context, requestServices, requestAborted) =>
         {
            var handlerExecutor = requestServices.GetService<ILambdaHandlerExecutor>();

            if (handlerExecutor == null)
            {
               throw new InvalidOperationException("No handler configured. Please specify a handler via ILambdaHostBuilder.UseHandler.");
            }

            return handlerExecutor.ExecuteAsync(inputStream, context, requestAborted);
         };

         for (var i = _middlewares.Count - 1; i >= 0; i--)
         {
            pipeline = _middlewares[i](pipeline);
         }

         var invocationMiddlewareFunc = MiddlewareExtensions.Resolve<InvocationMiddleware>();

         return invocationMiddlewareFunc(pipeline);
      }
   }
}
