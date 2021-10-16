using System.Collections.Generic;
using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Results
{
   public class HttpErrorResult : ILambdaResult
   {
      private readonly int _statusCode;
      private readonly string _message;

      public HttpErrorResult(int statusCode, string message)
      {
         _statusCode = statusCode;
         _message = message;
      }

      public Stream SerializeResult(ILambdaSerializer serializer, LambdaContext context)
      {
         // https://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-create-api-as-simple-proxy-for-lambda.html
         var response = new
         {
            StatusCode = _statusCode,
            Headers = new Dictionary<string, string>
            {
               {"X-Amz-Request-Id", context.AwsRequestId}
            },
            Body = _message
         };

         var responseStream = new MemoryStream();

         serializer.Serialize(response, responseStream);
         responseStream.Position = 0;

         return responseStream;
      }
   }
}
