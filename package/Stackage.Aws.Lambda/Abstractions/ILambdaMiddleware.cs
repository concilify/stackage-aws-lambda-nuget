using System.Threading.Tasks;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaMiddleware<TRequest>
   {
      Task<ILambdaResult> InvokeAsync(
         TRequest request,
         LambdaContext context,
         PipelineDelegate<TRequest> next);
   }
}
