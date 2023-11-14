using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaHandler<in TRequest>
   {
      Task<ILambdaResult> HandleAsync(TRequest request, ILambdaContext context);
   }
}
