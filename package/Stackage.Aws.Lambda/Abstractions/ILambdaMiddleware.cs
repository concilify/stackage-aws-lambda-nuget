using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaMiddleware<TRequest>
   {
      Task<ILambdaResult> InvokeAsync(
         TRequest request,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate<TRequest> next);
   }
}
