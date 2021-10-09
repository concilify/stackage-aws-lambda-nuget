using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ControllerTests
{
   public class FunctionsControllerTests
   {
      [Test]
      public async Task invocations_endpoint_returns_okay()
      {
         using var webApplicationFactory = new WebApplicationFactory<FakeRuntimeStartup>();
         using var httpClient = webApplicationFactory.CreateClient();

         var content = JsonContent.Create(new {foo = "bar"});

         var response = await httpClient.PostAsync("/2015-03-31/functions/my-function/invocations", content);

         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      [Test]
      public async Task invocations_endpoint_calls_functions_service_with_name_and_body()
      {
         var functionsService = A.Fake<IFunctionsService>();

         using var webApplicationFactory = new WebApplicationFactory<FakeRuntimeStartup>()
            .WithWebHostBuilder(builder =>
            {
               builder.ConfigureServices(services =>
               {
                  services.AddSingleton(functionsService);
               });
            });
         using var httpClient = webApplicationFactory.CreateClient();

         var content = JsonContent.Create(new {foo = "bar"});

         var response = await httpClient.PostAsync("/2015-03-31/functions/my-function/invocations", content);

         A.CallTo(() => functionsService.Invoke("my-function", "{\"foo\":\"bar\"}")).MustHaveHappenedOnceExactly();
      }
   }
}
