using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.FakeRuntime.Controllers
{
   [ApiController]
   [Route("2015-03-31/functions")]
   public class FunctionsController : ControllerBase
   {
      private readonly IFunctionsService _functionsService;

      public FunctionsController(IFunctionsService functionsService)
      {
         _functionsService = functionsService;
      }

      [HttpPost("{functionName}/invocations")]
      public async Task<IActionResult> InvocationsAsync(string functionName)
      {
         // X-Amz-Invocation-Type DryRun, Event, RequestResponse

         using (var reader = new StreamReader(Request.Body))
         {
            _functionsService.Invoke(functionName, await reader.ReadToEndAsync());
         }

         // The HTTP status code is in the 200 range for a successful request. For the RequestResponse invocation type,
         // this status code is 200. For the Event invocation type, this status code is 202. For the DryRun invocation type, the status code is 204.

         return Ok();
      }
   }
}
