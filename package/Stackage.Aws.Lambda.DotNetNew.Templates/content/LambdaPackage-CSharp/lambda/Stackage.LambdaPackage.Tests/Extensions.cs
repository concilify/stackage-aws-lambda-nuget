using System.IO;
using System.Threading.Tasks;

namespace Stackage.LambdaPackage.Tests
{
   public static class Extensions
   {
      public static async Task<string> ReadToEndAsync(this Stream stream)
      {
         using var reader = new StreamReader(stream);

         return await reader.ReadToEndAsync();
      }
   }
}
