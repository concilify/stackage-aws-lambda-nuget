using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Middleware
{
   public class RequestLoggingMiddleware<TRequest> : ILambdaMiddleware<TRequest>
   {
      private readonly ILogger<RequestLoggingMiddleware<TRequest>> _logger;

      public RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware<TRequest>> logger)
      {
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         TRequest request,
         ILambdaContext context,
         IServiceProvider requestServices,
         PipelineDelegate next)
      {
         using (_logger.BeginScope(new Dictionary<string, object> {{"AwsRequestId", context.AwsRequestId}}))
         {
            _logger.LogInformation("Request started");

            var timer = Stopwatch.StartNew();

            try
            {
               return await next(request, context, requestServices);
            }
            finally
            {
               _logger.LogInformation("Request completed in {durationMs}ms", timer.ElapsedMilliseconds);
            }
         }
      }
   }
}
