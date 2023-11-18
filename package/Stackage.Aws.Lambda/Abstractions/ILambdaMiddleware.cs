using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaMiddleware
   {
      Task<ILambdaResult> InvokeAsync(
         Stream request,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate next,
         CancellationToken cancellationToken = default);
   }
}
