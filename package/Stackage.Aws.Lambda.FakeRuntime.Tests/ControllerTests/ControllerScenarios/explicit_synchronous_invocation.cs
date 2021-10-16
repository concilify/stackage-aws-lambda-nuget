using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Lambda.FakeRuntime.Services;
using Stackage.Aws.Lambda.FakeRuntime.Tests.Stubs;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ControllerTests.ControllerScenarios
{
   public class explicit_synchronous_invocation
   {
      private string _awsRequestId;
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         _awsRequestId = Guid.NewGuid().ToString();

         var idGenerator = new StubIdGenerator(_awsRequestId);

         using var webApplicationFactory = new WebApplicationFactory<FakeRuntimeStartup>()
            .WithWebHostBuilder(builder =>
            {
               builder.ConfigureServices(services =>
               {
                  services.AddSingleton<IGenerateIds>(idGenerator);
               });
            });
         using var httpClient = webApplicationFactory.CreateClient();

         BackgroundInvocationHandler.HandleSingleInvocation(httpClient, "my-function", new {hello = "world"});

         var content = JsonContent.Create(new {foo = "bar"});
         content.Headers.Add("X-Amz-Invocation-Type", "RequestResponse");

         _response = await httpClient.PostAsync("/2015-03-31/functions/my-function/invocations", content);
      }

      [Test]
      public void endpoint_returns_200_okay()
      {
         Assert.That(_response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
      }

      [Test]
      public async Task endpoint_returns_content()
      {
         Assert.That(await _response.Content.ReadAsStringAsync(), Is.EqualTo("{\"hello\":\"world\"}"));
      }

      [Test]
      public void endpoint_returns_json_content_type()
      {
         Assert.That(_response.Content.Headers.GetValues("Content-Type").Single(), Is.EqualTo("application/json"));
      }

      [Test]
      public void endpoint_returns_request_id()
      {
         Assert.That(_response.Headers.GetValues("x-amzn-RequestId").Single(), Is.EqualTo(_awsRequestId));
      }

      [Test]
      public void endpoint_returns_executed_version()
      {
         Assert.That(_response.Headers.GetValues("X-Amz-Executed-Version").Single(), Is.EqualTo("$LATEST"));
      }
   }
}
