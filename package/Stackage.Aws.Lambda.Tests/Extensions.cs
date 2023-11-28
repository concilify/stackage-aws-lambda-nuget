using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Stackage.Aws.Lambda.Tests
{
   public static class Extensions
   {
      public static string ReadToEnd(this Stream stream)
      {
         using var reader = new StreamReader(stream);

         return reader.ReadToEnd();
      }

      public static async Task<string> ReadToEndAsync(this Stream stream)
      {
         using var reader = new StreamReader(stream);

         return await reader.ReadToEndAsync();
      }

      public static Stream ToStream(this string @string)
      {
         return new MemoryStream(Encoding.UTF8.GetBytes(@string));
      }
   }
}
