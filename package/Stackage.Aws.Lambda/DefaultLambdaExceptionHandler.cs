using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class DefaultLambdaExceptionHandler : ILambdaExceptionHandler
   {
      private readonly ILogger<DefaultLambdaExceptionHandler> _logger;

      public DefaultLambdaExceptionHandler(
         ILogger<DefaultLambdaExceptionHandler> logger)
      {
         _logger = logger;
      }

      [DoesNotReturn]
      public Stream HandleException(Exception exception)
      {
         _logger.LogError(exception, "An unhandled exception occured");

         throw new UnhandledException("An unhandled exception occured", exception);
      }
   }
}
