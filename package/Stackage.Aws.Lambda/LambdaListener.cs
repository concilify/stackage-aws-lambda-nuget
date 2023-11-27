using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   internal class LambdaListener : ILambdaListener
   {
      private readonly ILambdaRuntime _lambdaRuntime;
      private readonly IServiceProvider _serviceProvider;
      private readonly PipelineDelegate _pipelineAsync;

      public LambdaListener(
         ILambdaRuntime lambdaRuntime,
         IServiceProvider serviceProvider,
         PipelineDelegate pipelineAsync)
      {
         _lambdaRuntime = lambdaRuntime;
         _serviceProvider = serviceProvider;
         _pipelineAsync = pipelineAsync;
      }

      public async Task ListenAsync(CancellationToken cancellationToken)
      {
         while (!cancellationToken.IsCancellationRequested)
         {
            try
            {
               using var invocation = await _lambdaRuntime.WaitForInvocationAsync(cancellationToken);

               await InvokeAndReplyAsync(invocation, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
         }
      }

      internal async Task InvokeAndReplyAsync(ILambdaInvocation invocation, CancellationToken cancellationToken)
      {
         using var scope = _serviceProvider.CreateScope();

         var lambdaResult = await _pipelineAsync(invocation.InputStream, invocation.Context, scope.ServiceProvider, cancellationToken);

         await lambdaResult.ExecuteResultAsync(invocation.Context, scope.ServiceProvider);
      }
   }
}
