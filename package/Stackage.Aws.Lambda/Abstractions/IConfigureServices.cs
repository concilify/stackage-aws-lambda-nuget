using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface IConfigureServices
   {
      void ConfigureServices(IServiceCollection services);
   }
}
