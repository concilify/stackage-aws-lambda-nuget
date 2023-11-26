using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Extensions
{
   public static class LambdaPipelineBuilderExtensions
   {
      public static ILambdaPipelineBuilder Use<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]TMiddleware>(this ILambdaPipelineBuilder pipelineBuilder)
         where TMiddleware : ILambdaMiddleware
      {
         return pipelineBuilder.Use(next =>
         {
            return (inputStream, context, requestServices, requestAborted) =>
            {
               var middleware = ActivatorUtilities.CreateInstance<TMiddleware>(requestServices);

               if (middleware == null)
               {
                  throw new InvalidOperationException($"Failed to create instance of middleware type {typeof(TMiddleware).Name}");
               }

               return middleware.InvokeAsync(inputStream, context, requestServices, next, requestAborted);
            };
         });
      }
   }
}
