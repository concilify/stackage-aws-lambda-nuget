using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results
{
   public class ContentResult<TContent> : ILambdaResult
   {
      private readonly TContent _content;

      public ContentResult(TContent content)
      {
         _content = content;
      }

      public Stream SerializeResult(ILambdaSerializer serializer)
      {
         var responseStream = new MemoryStream();

         serializer.Serialize(_content, responseStream);
         responseStream.Position = 0;

         return responseStream;
      }
   }
}
