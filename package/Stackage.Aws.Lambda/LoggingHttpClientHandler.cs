using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Stackage.Aws.Lambda
{
   public class LoggingHttpClientHandler : DelegatingHandler
   {
      private readonly ILogger<LoggingHttpClientHandler> _logger;

      public LoggingHttpClientHandler(
         HttpMessageHandler innerHandler,
         ILogger<LoggingHttpClientHandler> logger)
         : base(innerHandler)
      {
         _logger = logger;
      }

      protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      {
         _logger.LogInformation("Sending HTTP request {HttpMethod} {Uri}", request.Method, request.RequestUri);

         var stopwatch = Stopwatch.StartNew();

         HttpResponseMessage response;

         using (_logger.BeginScope(new Dictionary<string, object?> {{"HttpMethod", request.Method}, {"Uri", request.RequestUri}}))
         {
            try
            {
               response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception e)
            {
               _logger.LogError(e, "HTTP request failed {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
               throw;
            }

            _logger.LogInformation("HTTP request completed {ElapsedMilliseconds}ms - {StatusCode}", stopwatch.ElapsedMilliseconds, (int)response.StatusCode);
         }

         return response;
      }
   }
}
