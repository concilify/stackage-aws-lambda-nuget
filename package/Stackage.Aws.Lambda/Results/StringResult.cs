using System;
using System.IO;
using System.Text;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results
{
   public class StringResult : ILambdaResult
   {
      private readonly string _content;

      public StringResult(string content)
      {
         _content = content ?? throw new ArgumentNullException(nameof(content));
      }

      public Stream SerializeResult(ILambdaSerializer serializer, ILambdaContext context)
      {
         return new MemoryStream(Encoding.UTF8.GetBytes(_content));
      }
   }
}
