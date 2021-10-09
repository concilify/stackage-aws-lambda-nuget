using System.IO;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class StreamParser : IRequestParser<Stream>
   {
      public Stream Parse(Stream stream)
      {
         return stream;
      }
   }
}
