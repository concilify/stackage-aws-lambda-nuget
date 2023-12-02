using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results;

public class StreamResult : ILambdaResult
{
   public StreamResult(Stream outputStream)
   {
      Content = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
   }

   public Stream Content { get; }

   public Task ExecuteResultAsync(ILambdaContext context, IServiceProvider requestServices)
   {
      var executor = requestServices.GetRequiredService<ILambdaResultExecutor<StreamResult>>();
      return executor.ExecuteAsync(context, this);
   }

   internal class Executor : ILambdaResultExecutor<StreamResult>
   {
      private readonly ILambdaRuntime _lambdaRuntime;

      public Executor(ILambdaRuntime lambdaRuntime)
      {
         _lambdaRuntime = lambdaRuntime;
      }

      public async Task ExecuteAsync(ILambdaContext context, StreamResult result)
      {
         try
         {
            await _lambdaRuntime.ReplyWithInvocationSuccessAsync(result.Content, context);
         }
         finally
         {
            await result.Content.DisposeAsync();
         }
      }
   }
}
