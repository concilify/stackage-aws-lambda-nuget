using System.Threading;
using System.Threading.Tasks;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaListener<TRequest>
   {
      Task ListenAsync(CancellationToken cancellationToken);
   }
}
