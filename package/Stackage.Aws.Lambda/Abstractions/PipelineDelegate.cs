using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public delegate Task<ILambdaResult> PipelineDelegate<in TRequest>(
      TRequest request,
      ILambdaContext context,
      IServiceProvider requestServices);
}
