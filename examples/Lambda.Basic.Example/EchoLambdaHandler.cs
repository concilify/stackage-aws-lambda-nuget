using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Lambda.Basic.Example
{
   public class EchoLambdaHandler : ILambdaHandler<Stream>
   {
      private readonly ILogger<EchoLambdaHandler> _logger;

      public EchoLambdaHandler(ILogger<EchoLambdaHandler> logger)
      {
         _logger = logger;
      }

      public async Task<ILambdaResult> HandleAsync(Stream input, ILambdaContext context)
      {
         using var reader = new StreamReader(input);

         var payload = await reader.ReadToEndAsync();

         _logger.LogInformation("Request handled {payload}", payload);

         return new StringResult(payload);
      }
   }
}
