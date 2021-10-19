using System.Threading;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface IDeadlineCancellation
   {
      CancellationToken Token { get; }
   }
}
