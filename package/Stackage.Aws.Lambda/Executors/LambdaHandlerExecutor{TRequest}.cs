using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Executors;

internal class LambdaHandlerExecutor<TInput> : ILambdaHandlerExecutor
{
   private readonly ILambdaSerializer _serializer;
   private readonly ILambdaHandler<TInput> _handler;

   public LambdaHandlerExecutor(
      ILambdaSerializer serializer,
      ILambdaHandler<TInput> handler)
   {
      _serializer = serializer;
      _handler = handler;
   }

   public async Task<ILambdaResult> ExecuteAsync(Stream inputStream, ILambdaContext context)
   {
      var inputObject = _serializer.Deserialize<TInput>(inputStream);

      return await _handler.HandleAsync(inputObject, context);
   }
}
