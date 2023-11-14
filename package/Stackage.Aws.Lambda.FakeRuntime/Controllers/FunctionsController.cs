using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.FakeRuntime.Controllers
{
   [ApiController]
   [Route("2015-03-31/functions")]
   public class FunctionsController : ControllerBase
   {
      private readonly IFunctionsService _functionsService;
      private readonly IGenerateIds _idGenerator;

      public FunctionsController(
         IFunctionsService functionsService,
         IGenerateIds idGenerator)
      {
         _functionsService = functionsService;
         _idGenerator = idGenerator;
      }

      [HttpPost("{functionName}/invocations")]
      public async Task<IActionResult> InvocationsAsync(string functionName)
      {
         var invocationType = GetInvocationType();

         if (invocationType == "DryRun")
         {
            Response.Headers.Add("x-amzn-RequestId", _idGenerator.Generate());
            return NoContent();
         }

         if (invocationType != "RequestResponse" && invocationType != "Event")
         {
            return BadRequest("X-Amz-Invocation-Type must be one of RequestResponse, Event or DryRun");
         }

         // TODO: Return error if function didn't initialise

         LambdaRequest request;

         using (var reader = new StreamReader(Request.Body))
         {
            request = _functionsService.Invoke(functionName, await reader.ReadToEndAsync());
         }

         Response.Headers.Add("x-amzn-RequestId", request.AwsRequestId);

         if (invocationType == "Event")
         {
            return Accepted();
         }

         if (!await request.WaitForCompletion(HttpContext.RequestAborted))
         {
            // TODO: Cancel the function invocation

            return StatusCode(499);
         }

         var completion = _functionsService.GetCompletion(functionName, request.AwsRequestId);

         Response.Headers.Add("X-Amz-Executed-Version", "$LATEST");

         return Content(completion.ResponseBody, "application/json");
      }

      private string GetInvocationType()
      {
         var invocationType = Request.Headers["X-Amz-Invocation-Type"];

         if (invocationType.Count == 0)
         {
            return "RequestResponse";
         }

         return invocationType.ToString();
      }
   }
}
