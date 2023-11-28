using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Exceptions;

namespace Stackage.Aws.Lambda.Results;

public class ExceptionResult : ILambdaResult
{
   public ExceptionResult(Exception exception)
   {
      Exception = exception;
   }

   internal ExceptionResult(string message)
   {
      Exception = new UnhandledError(message ?? throw new ArgumentNullException(nameof(message)));
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
