using System.Threading.Tasks;

namespace Stackage.Aws.Lambda.Abstractions
{
   public delegate Task<ILambdaResult> PipelineDelegate<in TRequest>(
      TRequest request,
      LambdaContext context);
}
