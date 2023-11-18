using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Executors;

internal class StreamLambdaHandlerExecutor : ILambdaHandlerExecutor
{
   private readonly ILambdaHandler<Stream> _handler;

   public StreamLambdaHandlerExecutor(ILambdaHandler<Stream> handler)
   {
      _handler = handler;
   }

   public async Task<ILambdaResult> ExecuteAsync(Stream request, ILambdaContext context, CancellationToken cancellationToken = default)
   {
      return await _handler.HandleAsync(request, context);
   }
}
