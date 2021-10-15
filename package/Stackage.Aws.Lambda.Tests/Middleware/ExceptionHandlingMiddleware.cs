using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Tests.Middleware
{
   public class ExceptionHandlingMiddleware<TRequest> : ILambdaMiddleware<TRequest>
   {
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
            return new ErrorResult(e.Message);
         }
      }

      private class ErrorResult : ILambdaResult
      {
         private readonly string _message;

         public ErrorResult(string message)
         {
            _message = message;
         }

         public Stream SerializeResult(ILambdaSerializer serializer)
         {
            return $"An error occurred - {_message}".ToStream();
         }
      }
   }
}
