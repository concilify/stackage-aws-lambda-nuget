using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Executors;

internal interface ILambdaHandlerExecutor
{
   Task<ILambdaResult> ExecuteAsync(
      Stream request,
      ILambdaContext context,
      CancellationToken cancellationToken = default);
}
