using System.Threading.Tasks;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaHandler<in TRequest>
   {
      Task<ILambdaResult> HandleAsync(TRequest request, LambdaContext context);
   }
}
