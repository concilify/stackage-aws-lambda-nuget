using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Extensions;

public static class MiddlewareExtensions
{
   public static Func<PipelineDelegate, PipelineDelegate> Resolve<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMiddleware>()
      where TMiddleware : ILambdaMiddleware
   {
      return next =>
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
      };
   }
}
