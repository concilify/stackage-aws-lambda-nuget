using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Middleware;

public class CommitShaScopeMiddleware : ILambdaMiddleware
{
   private readonly IConfiguration _configuration;
   private readonly ILogger<CommitShaScopeMiddleware> _logger;

   public CommitShaScopeMiddleware(
      IConfiguration configuration,
      ILogger<CommitShaScopeMiddleware> logger)
   {
      _configuration = configuration;
      _logger = logger;
   }

   public async Task<ILambdaResult> InvokeAsync(
      Stream inputStream,
      ILambdaContext context,
      IServiceProvider requestServices,
      PipelineDelegate next,
      CancellationToken cancellationToken)
   {
      using var _ = _logger.BeginScope(new { CommitSha = _configuration["COMMIT_SHA"] ?? "n/a" });
      // using var _ = _logger.BeginScope(new Dictionary<string, object>
      // {
      //    ["CommitSha"] = _configuration["COMMIT_SHA"] ?? "n/a"
      // });

      return await next(inputStream, context, requestServices, cancellationToken);
   }
}
