using System;
using System.Threading.Tasks;
using Lambda.Middleware.Example.Integrations;
using Lambda.Middleware.Example.Model;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;

namespace Lambda.Middleware.Example.Handler
{
   public class ObjectLambdaHandler : ILambdaHandler<InputPoco>
   {
      private readonly IDeadlineCancellation _deadlineCancellation;

      public ObjectLambdaHandler(IDeadlineCancellation deadlineCancellation)
      {
         _deadlineCancellation = deadlineCancellation;
      }

      public async Task<ILambdaResult> HandleAsync(InputPoco request, LambdaContext context)
      {
         if (request.Action == "throw")
         {
            throw new Exception("Throwing exception from ObjectLambdaHandler");
         }

         if (request.Action == "delay")
         {
            await Task.Delay(10000, _deadlineCancellation.Token);
         }

         return new HttpContentResult<OutputPoco>(new OutputPoco {Action = request.Action});
      }
   }
}
