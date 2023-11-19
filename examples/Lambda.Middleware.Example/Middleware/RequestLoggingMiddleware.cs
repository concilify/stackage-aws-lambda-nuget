using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Middleware
{
   public class RequestLoggingMiddleware : ILambdaMiddleware
   {
      private readonly ILogger<RequestLoggingMiddleware> _logger;

      public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger)
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
         var timer = Stopwatch.StartNew();

         var lambdaResult = await next(inputStream, context, requestServices, cancellationToken);

         _logger.LogInformation(
            "Request returned {lambdaResultType} in {durationMs}ms",
            lambdaResult.GetType().Name,
            timer.ElapsedMilliseconds);

         return lambdaResult;
      }
   }
}
