using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ControllerScenarios
{
   public class explicit_synchronous_invocation
   {
      private HttpResponseMessage _response;

      [OneTimeSetUp]
      public async Task setup_scenario()
      {
         using var webApplicationFactory = new WebApplicationFactory<FakeRuntimeStartup>();
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
   }
}
