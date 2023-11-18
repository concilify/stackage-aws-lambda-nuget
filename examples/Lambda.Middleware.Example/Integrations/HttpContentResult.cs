using System.Collections.Generic;
using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Integrations
{
   public class HttpContentResult<TContent> : ILambdaResult
   {
      private readonly TContent _content;
      private readonly int _statusCode;

      public HttpContentResult(TContent content, int statusCode = 200)
      {
         _content = content;
         _statusCode = statusCode;
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
            Body = _content
         };

         var outputStream = new MemoryStream();

         serializer.Serialize(response, outputStream);
         outputStream.Position = 0;

         return outputStream;
      }
   }
}
