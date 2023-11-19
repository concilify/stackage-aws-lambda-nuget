using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions;

public interface ILambdaResultExecutor<in TLambdaResult>
   where TLambdaResult : ILambdaResult
{
   Task ExecuteAsync(ILambdaContext context, TLambdaResult result);
}
