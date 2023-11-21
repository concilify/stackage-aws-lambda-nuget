using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Lambda.Middleware.Example.Integrations;
using Lambda.Middleware.Example.Model;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Handler
{
   public class ObjectLambdaHandler : ILambdaHandler<InputPoco>
   {
      private readonly IDeadlineCancellation _deadlineCancellation;

      public ObjectLambdaHandler(IDeadlineCancellation deadlineCancellation)
      {
         _deadlineCancellation = deadlineCancellation;
      }

      public async Task<ILambdaResult> HandleAsync(InputPoco input, ILambdaContext context)
      {
         if (input.Action == "throw")
         {
            throw new Exception("Throwing exception from ObjectLambdaHandler");
         }

         if (input.Action == "delay")
         {
            await Task.Delay(5000, _deadlineCancellation.Token);
         }

         return new ObjectResult(new OutputPoco {Action = input.Action});
      }
   }
}
