using System.Threading.Tasks;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.Handlers
{
   public class LongRunningObjectLambdaHandler : ILambdaHandler<StringPoco>
   {
      private readonly IDeadlineCancellation _deadlineCancellation;

      public LongRunningObjectLambdaHandler(IDeadlineCancellation deadlineCancellation)
      {
         _deadlineCancellation = deadlineCancellation;
      }

      public async Task<ILambdaResult> HandleAsync(StringPoco request, LambdaContext context)
      {
         await Task.Delay(10000, _deadlineCancellation.Token);

         return new StringResult("Should be timed-out");
      }
   }
}
