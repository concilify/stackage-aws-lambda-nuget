using System.Threading.Tasks;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.Handlers
{
   public class DecorateObjectLambdaHandler : ILambdaHandler<StringPoco>
   {
      public Task<ILambdaResult> HandleAsync(StringPoco request, LambdaContext context)
      {
         return Task.FromResult<ILambdaResult>(new ContentResult<StringPoco>(new StringPoco {Value = $"[{request.Value}]"}));
      }
   }
}
