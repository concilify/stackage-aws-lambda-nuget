using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Middleware
{
   public class ExceptionHandlingMiddleware : ILambdaMiddleware
   {
      private readonly ILogger<ExceptionHandlingMiddleware> _logger;

      public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
      {
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         Stream inputStream,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate next,
         CancellationToken cancellationToken)
      {
         try
         {
            return await next(inputStream, context, requestServices, cancellationToken);
         }
         catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
         {
            throw;
         }
         catch (Exception e)
         {
            _logger.LogError(e, "An unhandled exception occured");

            return new ExceptionResult(e);
         }
      }
   }
}
