using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results;

public class CancellationResult : ILambdaResult
{
   public CancellationResult(string message)
   {
      Message = message ?? throw new ArgumentNullException(nameof(message));
   }

   public string Message { get; }

   public Task ExecuteResultAsync(ILambdaContext context, IServiceProvider requestServices)
   {
      var executor = requestServices.GetRequiredService<ILambdaResultExecutor<CancellationResult>>();
      return executor.ExecuteAsync(context, this);
   }

   internal class Executor : ILambdaResultExecutor<CancellationResult>
   {
      private readonly ILambdaRuntime _lambdaRuntime;

      public Executor(ILambdaRuntime lambdaRuntime)
      {
         _lambdaRuntime = lambdaRuntime;
      }

      public async Task ExecuteAsync(ILambdaContext context, CancellationResult result)
      {
         await _lambdaRuntime.ReplyWithInvocationFailureAsync(new TaskCanceledException(result.Message), context);
      }
   }
}
