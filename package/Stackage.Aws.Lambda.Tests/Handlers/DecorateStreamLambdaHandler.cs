using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.Handlers
{
   public class DecorateStreamLambdaHandler : ILambdaHandler<Stream>
   {
      public async Task<ILambdaResult> HandleAsync(Stream input, ILambdaContext context)
      {
         var response = $"[{await input.ReadToEndAsync()}]";

         return new StreamResult(response.ToStream());
      }
   }
}
