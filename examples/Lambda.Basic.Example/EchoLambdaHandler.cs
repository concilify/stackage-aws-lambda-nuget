using System.IO;
using System.Text;
using System.Threading.Tasks;
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

      public async Task<ILambdaResult> HandleAsync(Stream request, LambdaContext context)
      {
         using var reader = new StreamReader(request);

         var payload = await reader.ReadToEndAsync();

         _logger.LogInformation("Request handled {payload}", payload);

         var output = new MemoryStream(Encoding.UTF8.GetBytes(payload));

         return new StreamResult(output);
      }
   }
}
