using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ControllerTests
{
   public class FunctionsControllerTests
   {
      [TestCase("foo")]
      [TestCase("ResponseRequest")]
      [TestCase("RunDry")]
      public async Task invocations_endpoint_returns_bad_request_for_invalid_invocation_types(string invocationType)
      {
         using var webApplicationFactory = new WebApplicationFactory<FakeRuntimeStartup>();
         using var httpClient = webApplicationFactory.CreateClient();

         var content = JsonContent.Create(new {foo = "bar"});
         content.Headers.Add("X-Amz-Invocation-Type", invocationType);

         var response = await httpClient.PostAsync("/2015-03-31/functions/my-function/invocations", content);

         Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
      }
   }
}
