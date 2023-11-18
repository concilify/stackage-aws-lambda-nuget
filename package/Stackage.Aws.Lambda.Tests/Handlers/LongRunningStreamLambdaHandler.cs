using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.Handlers
{
   public class LongRunningStreamLambdaHandler : ILambdaHandler<Stream>
   {
      private readonly IDeadlineCancellation _deadlineCancellation;

      public LongRunningStreamLambdaHandler(IDeadlineCancellation deadlineCancellation)
      {
         _deadlineCancellation = deadlineCancellation;
      }

      public async Task<ILambdaResult> HandleAsync(Stream inputStream, ILambdaContext context)
      {
         await Task.Delay(10000, _deadlineCancellation.Token);

         return new StringResult("Should be timed-out");
      }
   }
}
