using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public delegate Task<ILambdaResult> PipelineDelegate(
      Stream request,
      ILambdaContext context,
      IServiceProvider serviceProvider);
}
