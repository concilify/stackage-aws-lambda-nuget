using System.Threading;
using System.Threading.Tasks;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaListener
   {
      Task ListenAsync(CancellationToken cancellationToken);
   }
}
