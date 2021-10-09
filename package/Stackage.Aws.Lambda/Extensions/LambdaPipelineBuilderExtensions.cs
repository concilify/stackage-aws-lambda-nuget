using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Extensions
{
   public static class LambdaPipelineBuilderExtensions
   {
      public static ILambdaPipelineBuilder<Stream> Use<TMiddleware>(this ILambdaPipelineBuilder<Stream> pipelineBuilder)
         where TMiddleware : ILambdaMiddleware<Stream>
      {
         return pipelineBuilder.Use(next =>
         {
            return (request, context) =>
            {
               var middleware = ActivatorUtilities.CreateInstance<TMiddleware>(context.RequestServices);

               if (middleware == null)
               {
                  throw new InvalidOperationException($"Failed to create instance of middleware type {typeof(TMiddleware).Name}");
               }

               return middleware.InvokeAsync(request, context, next);
            };
         });
      }

      public static ILambdaPipelineBuilder<TRequest> Use<TMiddleware, TRequest>(this ILambdaPipelineBuilder<TRequest> pipelineBuilder)
         where TMiddleware : ILambdaMiddleware<TRequest>
      {
         return pipelineBuilder.Use(next =>
         {
            return (request, context) =>
            {
               var middleware = ActivatorUtilities.CreateInstance<TMiddleware>(context.RequestServices);

               if (middleware == null)
               {
                  throw new InvalidOperationException($"Failed to create instance of middleware type {typeof(TMiddleware).Name}");
               }

               return middleware.InvokeAsync(request, context, next);
            };
         });
      }
   }
}
