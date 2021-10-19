using System.Threading;

namespace Stackage.Aws.Lambda.Middleware
{
   public interface IDeadlineCancellationInitializer
   {
      void Initialize(CancellationToken token);
   }
}
