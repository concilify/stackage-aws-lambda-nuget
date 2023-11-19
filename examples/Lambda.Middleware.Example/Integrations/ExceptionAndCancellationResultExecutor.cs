using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Lambda.Middleware.Example.Integrations;

public class ExceptionAndCancellationResultExecutor : ILambdaResultExecutor<ExceptionResult>, ILambdaResultExecutor<CancellationResult>
{
   private readonly ILambdaSerializer _serializer;
   private readonly IRuntimeApiClient _runtimeApiClient;

   public ExceptionAndCancellationResultExecutor(
      ILambdaSerializer serializer,
      IRuntimeApiClient runtimeApiClient)
   {
      _serializer = serializer;
      _runtimeApiClient = runtimeApiClient;
   }

   public async Task ExecuteAsync(ILambdaContext context, ExceptionResult result)
   {
      await ExecuteAsync(context, 500, "Internal Server Error");
   }

   public async Task ExecuteAsync(ILambdaContext context, CancellationResult result)
   {
      await ExecuteAsync(context, 499, "Client Closed Request");
   }

   private async Task ExecuteAsync(ILambdaContext context, int statusCode, string message)
   {
      var responseObject = new
      {
         StatusCode = statusCode,
         Headers = new Dictionary<string, string>
         {
            { "x-amzn-RequestId", context.AwsRequestId }
         },
         Body = message
      };

      using var response = new MemoryStream();

      _serializer.Serialize(responseObject, response);
      response.Position = 0;

      await _runtimeApiClient.SendResponseAsync(context.AwsRequestId, response, CancellationToken.None);
   }
}
