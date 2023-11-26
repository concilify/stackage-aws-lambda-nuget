using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Options;

namespace Stackage.Aws.Lambda.Extensions
{
   public static class ServicesExtensions
   {
      public static void AddDeadlineCancellation(this IServiceCollection services, IConfiguration configuration)
      {
         services.AddScoped<DeadlineCancellation>();
         services.AddScoped<IDeadlineCancellationInitializer>(sp => sp.GetRequiredService<DeadlineCancellation>());
         services.AddScoped<IDeadlineCancellation>(sp => sp.GetRequiredService<DeadlineCancellation>());

         services.Configure<DeadlineCancellationOptions>(configuration.GetSection("DeadlineCancellation"));
      }
   }
}
