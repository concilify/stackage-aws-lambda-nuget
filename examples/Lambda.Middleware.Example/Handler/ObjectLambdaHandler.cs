using System;
using System.Threading.Tasks;
using Lambda.Middleware.Example.Model;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Handler
{
   public class ObjectLambdaHandler : ILambdaHandler<InputPoco>
   {
      public async Task<ILambdaResult> HandleAsync(InputPoco request, LambdaContext context)
      {
         await Task.Yield();

         if (request.Value == "throw")
         {
            throw new Exception("Throwing exception from ObjectLambdaHandler");
         }

         return new ContentResult<OutputPoco>(new OutputPoco {Value = request.Value});
      }
   }
}
