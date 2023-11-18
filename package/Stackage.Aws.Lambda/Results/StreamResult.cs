using System;
using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results
{
   public class StreamResult : ILambdaResult
   {
      private readonly Stream _outputStream;

      public StreamResult(Stream outputStream)
      {
         _outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
      }

      public Stream SerializeResult(ILambdaSerializer serializer, ILambdaContext context)
      {
         return _outputStream;
      }
   }
}
