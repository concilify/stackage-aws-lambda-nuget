using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
         LambdaContext context,
         PipelineDelegate<TRequest> next)
      {
         using (_logger.BeginScope(new Dictionary<string, object> {{"AwsRequestId", context.AwsRequestId}}))
         {
            _logger.LogInformation("Request started");

            var timer = Stopwatch.StartNew();

            try
            {
               return await next(request, context);
            }
            finally
            {
               _logger.LogInformation("Request completed in {durationMs}ms", timer.ElapsedMilliseconds);
            }
         }
      }
   }
}
