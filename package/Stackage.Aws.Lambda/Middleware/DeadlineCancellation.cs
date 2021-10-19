using System.Threading;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Middleware
{
   public class DeadlineCancellation : IDeadlineCancellation, IDeadlineCancellationInitializer
   {
      public CancellationToken Token { get; private set; }

      public void Initialize(CancellationToken token)
      {
         Token = token;
      }
   }
}
