using System;
using System.Threading.Tasks;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests.Handlers
{
   public class ThrowingObjectLambdaHandler : ILambdaHandler<StringPoco>
   {
      public Task<ILambdaResult> HandleAsync(StringPoco request, LambdaContext context)
      {
         throw new Exception("ThrowingObjectLambdaHandler failed");
      }
   }
}
