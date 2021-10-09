using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public interface IRequestHandler<out TRequest>
   {
      Task<Stream> HandleAsync(Stream requestStream, ILambdaContext context, PipelineDelegate<TRequest> pipelineAsync);
   }
}