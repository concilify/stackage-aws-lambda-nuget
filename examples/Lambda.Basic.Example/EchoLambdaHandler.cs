using System.IO;
using System.Threading.Tasks;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Lambda.Basic.Example
{
   public class EchoLambdaHandler : ILambdaHandler<Stream>
   {
      public async Task<ILambdaResult> HandleAsync(Stream request, LambdaContext context)
      {
         var output = new MemoryStream();

         await request.CopyToAsync(output);

         return new StreamResult(output);
      }
   }
}
