using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Middleware
{
   public class ExceptionHandlingMiddleware : ILambdaMiddleware
   {
      private readonly ILambdaResultFactory _resultFactory;
      private readonly ILogger<ExceptionHandlingMiddleware> _logger;

      public ExceptionHandlingMiddleware(
         ILambdaResultFactory resultFactory,
         ILogger<ExceptionHandlingMiddleware> logger)
      {
         _resultFactory = resultFactory;
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         Stream inputStream,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate next,
         CancellationToken cancellationToken = default)
      {
         try
         {
            return await next(inputStream, context, requestServices, cancellationToken);
         }
         catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
         {
            _logger.LogWarning("The request was forcibly ended by the host");

            return _resultFactory.HostEndedRequest();
         }
         catch (Exception e)
         {
            _logger.LogError(e, "An unhandled exception occured");

            return _resultFactory.UnhandledException(e);
         }
      }
   }
}
