using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface IRequestHandler<out TRequest>
   {
      Task<Stream> HandleAsync(
         Stream requestStream,
         ILambdaContext context,
         PipelineDelegate pipelineAsync);
   }
}
