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

      public Stream SerializeResult(ILambdaSerializer serializer, ILambdaContext context)
      {
         var outputStream = new MemoryStream();

         serializer.Serialize(_content, outputStream);
         outputStream.Position = 0;

         return outputStream;
      }
   }
}

