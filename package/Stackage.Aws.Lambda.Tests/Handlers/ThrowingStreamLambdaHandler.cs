using System;
using System.IO;
using System.Threading.Tasks;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Tests.Handlers
{
   public class ThrowingStreamLambdaHandler : ILambdaHandler<Stream>
   {
      public Task<ILambdaResult> HandleAsync(Stream request, LambdaContext context)
      {
         throw new Exception("ThrowingStreamLambdaHandler failed");
      }
   }
}
