using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Lambda.Middleware.Example.Model;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

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

         if (input.Action != null && input.Action.StartsWith("delay"))
         {
            await Task.Delay(ParseDelay(input.Action), _deadlineCancellation.Token);
         }

         return new ObjectResult(new OutputPoco {Action = input.Action});
      }

      private static int ParseDelay(string action)
      {
         var parts = action.Split(":");

         if (parts.Length <= 1 || !int.TryParse(parts[1], out var delay))
         {
            return 5000;
         }

         return delay;
      }
   }
}
