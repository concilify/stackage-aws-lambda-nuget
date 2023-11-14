using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.Handlers
{
   public class DecorateObjectLambdaHandler : ILambdaHandler<StringPoco>
   {
      public Task<ILambdaResult> HandleAsync(StringPoco request, ILambdaContext context)
      {
         return Task.FromResult<ILambdaResult>(new ContentResult<StringPoco>(new StringPoco {Value = $"[{request.Value}]"}));
      }
   }
}
