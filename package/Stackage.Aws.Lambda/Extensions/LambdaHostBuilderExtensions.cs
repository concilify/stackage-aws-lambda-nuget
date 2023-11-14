using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Extensions
{
   public static class LambdaHostBuilderExtensions
   {
      public static ILambdaHostBuilder UseHandler<THandler>(this ILambdaHostBuilder builder)
         where THandler : ILambdaHandler<Stream>
      {
         builder.UseHandler(CreateDelegate<THandler, Stream>);

         return builder;
      }

      public static ILambdaHostBuilder<TRequest> UseHandler<THandler, TRequest>(this ILambdaHostBuilder<TRequest> builder)
         where THandler : ILambdaHandler<TRequest>
      {
         builder.UseHandler(CreateDelegate<THandler, TRequest>);

         return builder;
      }

      private static PipelineDelegate<TRequest> CreateDelegate<THandler, TRequest>(IServiceProvider requestServices)
         where THandler : ILambdaHandler<TRequest>
      {
         var handler = ActivatorUtilities.CreateInstance<THandler>(requestServices);

         return async (stream, context, _) =>
         {
            var outputStream = await handler.HandleAsync(stream, context);

            return outputStream;
         };
      }
   }
}
