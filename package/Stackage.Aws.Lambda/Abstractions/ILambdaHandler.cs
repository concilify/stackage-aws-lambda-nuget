using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaHandler<in TInput>
   {
      Task<ILambdaResult> HandleAsync(TInput input, ILambdaContext context);
   }
}
