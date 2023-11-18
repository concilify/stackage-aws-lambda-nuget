using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Tests.Handlers
{
   public class ThrowingStreamLambdaHandler : ILambdaHandler<Stream>
   {
      public Task<ILambdaResult> HandleAsync(Stream inputStream, ILambdaContext context)
      {
         throw new Exception("ThrowingStreamLambdaHandler failed");
      }
   }
}
