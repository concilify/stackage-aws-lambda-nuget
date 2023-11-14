using System.Collections.Generic;
using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Integrations
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

      public Stream SerializeResult(ILambdaSerializer serializer, ILambdaContext context)
      {
         var response = new
         {
            StatusCode = _statusCode,
            Headers = new Dictionary<string, string>
            {
               {"x-amzn-RequestId", context.AwsRequestId}
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
