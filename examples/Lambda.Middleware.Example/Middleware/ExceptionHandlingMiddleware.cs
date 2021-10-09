using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Middleware
{
   public class ExceptionHandlingMiddleware<TRequest> : ILambdaMiddleware<TRequest>
   {
      private readonly ILogger<ExceptionHandlingMiddleware<TRequest>> _logger;

      public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware<TRequest>> logger)
      {
         _logger = logger;
      }

      public async Task<ILambdaResult> InvokeAsync(
         TRequest request,
         LambdaContext context,
         PipelineDelegate<TRequest> next)
      {
         try
         {
            return await next(request, context);
         }
         catch (Exception e)
         {
            _logger.LogError(e, "An unhandled exception occured");

            return new ErrorResult(500, "Internal Server Error", context.AwsRequestId);
         }
      }

      private class ErrorResult : ILambdaResult
      {
         private readonly int _statusCode;
         private readonly string _content;
         private readonly string _awsRequestId;

         public ErrorResult(int statusCode, string content, string awsRequestId)
         {
            _statusCode = statusCode;
            _content = content;
            _awsRequestId = awsRequestId;
         }

         public Stream SerializeResult(ILambdaSerializer serializer, LambdaContext context)
         {
            var response = new
            {
               StatusCode = _statusCode,
               Content = _content,
               AwsRequestId = _awsRequestId
            };

            var responseStream = new MemoryStream();

            serializer.Serialize(response, responseStream);
            responseStream.Position = 0;

            return responseStream;
         }
      }
   }
}
