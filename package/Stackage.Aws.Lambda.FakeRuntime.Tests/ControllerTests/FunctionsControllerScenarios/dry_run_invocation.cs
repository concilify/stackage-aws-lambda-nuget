using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Services;
using Stackage.Aws.Lambda.FakeRuntime.Tests.Stubs;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ControllerTests.FunctionsControllerScenarios
{
   public class dry_run_invocation
   {
      private string _awsRequestId;
      private IFunctionsService _functionsService;
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _awsRequestId = Guid.NewGuid().ToString();
         _functionsService = A.Fake<IFunctionsService>();

         var idGenerator = new StubIdGenerator(_awsRequestId);

         using var webApplicationFactory = new WebApplicationFactory<FakeRuntimeStartup>()
            .WithWebHostBuilder(builder =>
            {
               builder.ConfigureServices(services =>
               {
                  services.AddSingleton<IGenerateIds>(idGenerator);
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
      public async Task endpoint_returns_empty_content()
      {
         Assert.That(await _response.Content.ReadAsStringAsync(), Is.Empty);
      }

      [Test]
      public void endpoint_does_not_return_content_type()
      {
         Assert.That(_response.Content.Headers.Contains("Content-Type"), Is.False);
      }

      [Test]
      public void endpoint_returns_request_id()
      {
         Assert.That(_response.Headers.GetValues("x-amzn-RequestId").Single(), Is.EqualTo(_awsRequestId));
      }

      [Test]
      public void endpoint_does_not_return_executed_version()
      {
         Assert.That(_response.Headers.Contains("X-Amz-Executed-Version"), Is.False);
      }
   }
}
