using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ControllerScenarios
{
   public class dry_run_invocation
   {
      private IFunctionsService _functionsService;
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _functionsService = A.Fake<IFunctionsService>();

         using var webApplicationFactory = new WebApplicationFactory<FakeRuntimeStartup>()
            .WithWebHostBuilder(builder =>
            {
               builder.ConfigureServices(services =>
               {
                  services.AddSingleton(_functionsService);
               });
            });
         using var httpClient = webApplicationFactory.CreateClient();

         var content = JsonContent.Create(new {foo = "bar"});
         content.Headers.Add("X-Amz-Invocation-Type", "DryRun");

         _response = await httpClient.PostAsync("/2015-03-31/functions/my-function/invocations", content);
      }

      [Test]
      public void endpoint_does_not_call_functions_service()
      {
         A.CallTo(() => _functionsService.Invoke(A<string>._, A<string>._)).MustNotHaveHappened();
      }

      [Test]
      public void endpoint_does_not_call_get_completion()
      {
         A.CallTo(() => _functionsService.GetCompletion(A<string>._, A<string>._)).MustNotHaveHappened();
      }

      [Test]
      public void endpoint_returns_204_no_content()
      {
         Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
      }

      [Test]
      public void endpoint_returns_what_content()
      {
         Assert.Fail();
      }
   }
}
