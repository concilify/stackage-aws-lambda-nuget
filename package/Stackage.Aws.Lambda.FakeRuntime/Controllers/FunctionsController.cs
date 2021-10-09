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
         using (var reader = new StreamReader(Request.Body))
         {
            _functionsService.Invoke(functionName, await reader.ReadToEndAsync());
         }

         return Ok();
      }
   }
}
