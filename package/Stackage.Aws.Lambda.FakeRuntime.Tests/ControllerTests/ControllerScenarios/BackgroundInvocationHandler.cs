using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.ControllerTests.ControllerScenarios
{
   public static class BackgroundInvocationHandler
   {
      public static void HandleSingleInvocation<TResponse>(HttpClient httpClient, string functionName, TResponse response)
      {
         Task.Run(() => HandleInvocation<TResponse>(httpClient, functionName, response));
      }

      private static async Task HandleInvocation<TResponse>(HttpClient httpClient, string functionName, TResponse response)
      {
         var invocationResponse = await httpClient.GetAsync($"/{functionName}/2018-06-01/runtime/invocation/next");

         invocationResponse.EnsureSuccessStatusCode();

         var awsRequestId = invocationResponse.Headers.GetValues("Lambda-Runtime-Aws-Request-Id").Single();

         var content = JsonContent.Create(response);

         await httpClient.PostAsync($"/{functionName}/2018-06-01/runtime/invocation/{awsRequestId}/response", content);
      }

   }
}
