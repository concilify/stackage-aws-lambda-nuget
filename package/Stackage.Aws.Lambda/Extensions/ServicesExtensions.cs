using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;

namespace Stackage.Aws.Lambda.Extensions
{
   public static class ServicesExtensions
   {
      public static void AddDeadlineCancellation(this IServiceCollection services)
      {
         services.AddScoped<DeadlineCancellation>();
         services.AddScoped<IDeadlineCancellationInitializer>(sp => sp.GetRequiredService<DeadlineCancellation>());
         services.AddScoped<IDeadlineCancellation>(sp => sp.GetRequiredService<DeadlineCancellation>());
      }
   }
}
