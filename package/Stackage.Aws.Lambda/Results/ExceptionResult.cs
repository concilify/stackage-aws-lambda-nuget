using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results;

public class ExceptionResult : ILambdaResult
{
   public ExceptionResult(Exception exception)
   {
      Exception = exception ?? throw new ArgumentNullException(nameof(exception));
   }

   public Exception Exception { get; }

   public Task ExecuteResultAsync(ILambdaContext context, IServiceProvider requestServices)
   {
      var executor = requestServices.GetRequiredService<ILambdaResultExecutor<ExceptionResult>>();
      return executor.ExecuteAsync(context, this);
   }

   internal class Executor : ILambdaResultExecutor<ExceptionResult>
   {
      private readonly ILambdaRuntime _lambdaRuntime;

      public Executor(ILambdaRuntime lambdaRuntime)
      {
         _lambdaRuntime = lambdaRuntime;
      }

      public async Task ExecuteAsync(ILambdaContext context, ExceptionResult result)
      {
         await _lambdaRuntime.ReplyWithInvocationFailureAsync(result.Exception, context);
      }
   }
}
