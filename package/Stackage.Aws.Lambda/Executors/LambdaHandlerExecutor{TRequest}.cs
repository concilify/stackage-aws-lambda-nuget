using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Executors;

internal class LambdaHandlerExecutor<TRequest> : ILambdaHandlerExecutor
{
   private readonly ILambdaSerializer _serializer;
   private readonly ILambdaHandler<TRequest> _handler;

   public LambdaHandlerExecutor(
      ILambdaSerializer serializer,
      ILambdaHandler<TRequest> handler)
   {
      _serializer = serializer;
      _handler = handler;
   }

   public async Task<ILambdaResult> ExecuteAsync(Stream request, ILambdaContext context)
   {
      var deserializedRequest = _serializer.Deserialize<TRequest>(request);

      return await _handler.HandleAsync(deserializedRequest, context);
   }
}
