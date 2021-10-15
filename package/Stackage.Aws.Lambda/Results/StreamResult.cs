using System;
using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results
{
   public class StreamResult : ILambdaResult
   {
      private readonly Stream _stream;

      public StreamResult(Stream stream)
      {
         _stream = stream ?? throw new ArgumentNullException(nameof(stream));
      }

      public Stream SerializeResult(ILambdaSerializer serializer)
      {
         return _stream;
      }
   }
}
