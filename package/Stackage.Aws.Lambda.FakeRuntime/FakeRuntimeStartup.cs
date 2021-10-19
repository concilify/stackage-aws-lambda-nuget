using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.FakeRuntime.Services;
using Stackage.Core.Abstractions;
using Stackage.Core.Abstractions.Metrics;
using Stackage.Core.Extensions;
using Stackage.Core.Metrics;
using Stackage.Core.SystemTextJson;

namespace Stackage.Aws.Lambda.FakeRuntime
{
   public class FakeRuntimeStartup
   {
      private readonly IConfiguration _configuration;

      public FakeRuntimeStartup(IConfiguration configuration)
      {
         _configuration = configuration;
      }

      public void ConfigureServices(IServiceCollection services)
      {
         services.AddDefaultServices(_configuration);

         services.Configure<FakeRuntimeOptions>(_configuration.GetSection("FakeRuntimeOptions"));

         services.AddTransient<IJsonSerialiser, SystemTextJsonSerialiser>();
         services.AddTransient<IMetricSink, LoggingMetricSink>();

         services.AddTransient<IFunctionsService, FunctionsService>();
         services.AddTransient<IGenerateIds, IdGenerator>();

         services.AddSingleton<LambdaFunction.Dictionary>();

         services.AddControllers();
      }

      public void Configure(IApplicationBuilder app)
      {
         app.UseDefaultMiddleware();

         app.UseRouting();
         app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
      }
   }
}
