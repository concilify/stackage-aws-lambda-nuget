using System.Collections.Generic;
using System.IO;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Integrations
{
   public class HttpApiV2ContentResult<TContent> : ILambdaResult
   {
      private readonly TContent _content;
      private readonly int _statusCode;

      public HttpApiV2ContentResult(TContent content, int statusCode = 200)
      {
         _content = content;
         _statusCode = statusCode;
      }

      public Stream SerializeResult(ILambdaSerializer serializer, LambdaContext context)
      {
         // https://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-create-api-as-simple-proxy-for-lambda.html
         var response = new APIGatewayHttpApiV2ProxyResponse<TContent>
         {
            StatusCode = _statusCode,
            Headers = new Dictionary<string, string>
            {
               {"x-amzn-RequestId", context.AwsRequestId}
            },
            Body = _content
         };

         var responseStream = new MemoryStream();
         serializer.Serialize(response, responseStream);
         responseStream.Position = 0;

         return responseStream;
      }
   }
}
