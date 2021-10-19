using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Extensions
{
   public static class ServicesExtensions
   {
      public static void AddDeadlineCancellation(this IServiceCollection services)
      {
         services.TryAddSingleton<ILambdaResultFactory, DefaultLambdaResultFactory>();

         services.AddScoped<DeadlineCancellation>();
         services.AddScoped<IDeadlineCancellationInitializer>(sp => sp.GetRequiredService<DeadlineCancellation>());
         services.AddScoped<IDeadlineCancellation>(sp => sp.GetRequiredService<DeadlineCancellation>());
      }
   }
}
