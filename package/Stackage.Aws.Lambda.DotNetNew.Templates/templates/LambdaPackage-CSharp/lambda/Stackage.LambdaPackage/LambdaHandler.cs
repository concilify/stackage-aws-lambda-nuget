using System.Threading.Tasks;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.LambdaPackage
{
   // TODO: Rename and implement the required behaviour
   public class LambdaHandler : ILambdaHandler<Request>
   {
      public Task<ILambdaResult> HandleAsync(Request request, LambdaContext context)
      {
         ILambdaResult result = new StringResult($"Greetings {request.Name}!");

         return Task.FromResult(result);
      }
   }
}
