using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Handler
{
   public class ContentResult<TContent> : ILambdaResult
   {
      private readonly TContent _content;
      private readonly int _statusCode;

      public ContentResult(TContent content, int statusCode = 200)
      {
         _content = content;
         _statusCode = statusCode;
      }

      public Stream SerializeResult(ILambdaSerializer serializer, LambdaContext context)
      {
         var response = new
         {
            StatusCode = _statusCode,
            Content = _content
         };

         var responseStream = new MemoryStream();
         serializer.Serialize(response, responseStream);
         responseStream.Position = 0;

         return responseStream;
      }
   }
}
