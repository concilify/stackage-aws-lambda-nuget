using System;
using Microsoft.Extensions.Hosting;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Extensions
{
   public static class HostBuilderExtensions
   {
      public static IHostBuilder ConfigureLambdaHost(this IHostBuilder builder, Action<ILambdaHostBuilder> configure)
      {
         var lambdaHostBuilder = new LambdaHostBuilder(builder);

         configure(lambdaHostBuilder);

         return builder;
      }

      public static IHostBuilder ConfigureLambdaHost<TRequest>(this IHostBuilder builder, Action<ILambdaHostBuilder<TRequest>> configure)
      {
         var lambdaHostBuilder = new LambdaHostBuilder<TRequest>(builder);

         configure(lambdaHostBuilder);

         return builder;
      }
   }
}
