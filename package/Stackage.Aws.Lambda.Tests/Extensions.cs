using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Tests.Fakes;
using Stackage.Aws.Lambda.Tests.Fakes.Model;

namespace Stackage.Aws.Lambda.Tests
{
   public static class Extensions
   {
      public static void AddCapturingLogger(this LambdaListenerBuilder builder, IList<LogEntry> logs)
      {
         builder.ConfigureServices(services => services.AddSingleton<ILoggerProvider>(new CapturingLogger.Provider(logs)));
      }

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
