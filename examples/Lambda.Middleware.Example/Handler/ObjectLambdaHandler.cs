using System;
using System.Threading.Tasks;
using Lambda.Middleware.Example.Integrations;
using Lambda.Middleware.Example.Model;
using Lambda.Middleware.Example.Results;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Handler
{
   public class ObjectLambdaHandler : ILambdaHandler<InputPoco>
   {
      public async Task<ILambdaResult> HandleAsync(InputPoco request, LambdaContext context)
      {
         if (request.Action == "throw")
         {
            throw new Exception("Throwing exception from ObjectLambdaHandler");
         }

         if (request.Action == "delay")
         {
            await Task.Delay(1000);
         }

         return new HttpContentResult<OutputPoco>(new OutputPoco {Action = request.Action});
      }
   }
}
