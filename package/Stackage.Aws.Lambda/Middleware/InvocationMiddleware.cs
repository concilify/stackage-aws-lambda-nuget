using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Middleware;

public class InvocationMiddleware : ILambdaMiddleware
{
   private readonly ILogger<InvocationMiddleware> _logger;

   public InvocationMiddleware(ILogger<InvocationMiddleware> logger)
   {
      _logger = logger;
   }

   public async Task<ILambdaResult> InvokeAsync(
      Stream inputStream,
      ILambdaContext context,
      IServiceProvider requestServices,
      PipelineDelegate next,
      CancellationToken requestAborted)
   {
      using var _ = _logger.BeginScope("Handling request {AwsRequestId}", context.AwsRequestId);

      var stopwatch = Stopwatch.StartNew();

      try
      {
         var lambdaResult = await next(inputStream, context, requestServices, requestAborted);

         _logger.LogInformation("Request handler completed ({ElapsedMilliseconds}ms)", stopwatch.ElapsedMilliseconds);

         return lambdaResult;
      }
      catch (OperationCanceledException e) when (requestAborted.IsCancellationRequested)
      {
         _logger.LogWarning(e, "Request handler cancelled by host ({ElapsedMilliseconds}ms)", stopwatch.ElapsedMilliseconds);

         return new CancellationResult("The request was cancelled by the host; the handler may or may not have completed");
      }
      catch (Exception e)
      {
         _logger.LogError(e, "Request handler failed ({ElapsedMilliseconds}ms)", stopwatch.ElapsedMilliseconds);

         return new ExceptionResult("The request failed due to an unhandled error; the handler may or may not have completed");
      }
   }
}
