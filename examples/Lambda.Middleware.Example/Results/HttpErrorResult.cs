using System;
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
      private readonly string _awsRequestId;

      public HttpErrorResult(int statusCode, string message, string awsRequestId)
      {
         _statusCode = statusCode;
         _message = message;
         _awsRequestId = awsRequestId;
      }

      public Stream SerializeResult(ILambdaSerializer serializer)
      {
         // https://docs.aws.amazon.com/apigateway/latest/developerguide/api-gateway-create-api-as-simple-proxy-for-lambda.html
         var response = new
         {
            StatusCode = _statusCode,
            Headers = new Dictionary<string, string>
            {
               {"x-amz-request-id", _awsRequestId}
            },
            Body = _message
         };

         var responseStream = new MemoryStream();

         serializer.Serialize(response, responseStream);
         responseStream.Position = 0;

         return responseStream;
      }
   }

   public class HttpLambdaResultFactory : ILambdaResultFactory
   {
      public ILambdaResult UnhandledException(Exception exception, LambdaContext context)
      {
         return new HttpErrorResult(500, "Internal Server Error", context.AwsRequestId);
      }

      public ILambdaResult RemainingTimeExpired(LambdaContext context)
      {
         return new HttpErrorResult(499, "Client Closed Request", context.AwsRequestId);
      }
   }
}
