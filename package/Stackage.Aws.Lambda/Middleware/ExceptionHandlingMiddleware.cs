using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Middleware
{
   public class ExceptionHandlingMiddleware<TRequest> : ILambdaMiddleware<TRequest>
   {
      private readonly ILambdaResultFactory _resultFactory;
      private readonly ILogger<ExceptionHandlingMiddleware<TRequest>> _logger;

      public ExceptionHandlingMiddleware(
         ILambdaResultFactory resultFactory,
         ILogger<ExceptionHandlingMiddleware<TRequest>> logger)
      {
         _resultFactory = resultFactory;
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         TRequest request,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate<TRequest> next)
      {
         try
         {
            return await next(request, context, requestServices);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "An unhandled exception occured");

            return _resultFactory.UnhandledException(e);
         }
      }
   }
}
