using System.IO;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface IRequestParser<TRequest>
   {
      TRequest Parse(Stream stream);
   }
}
