using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results;

public class StringResult : ILambdaResult
{
   public StringResult(string content)
   {
      Content = content ?? throw new ArgumentNullException(nameof(content));
   }

   public string Content { get; }

   public Task ExecuteResultAsync(ILambdaContext context, IServiceProvider requestServices)
   {
      var executor = requestServices.GetRequiredService<ILambdaResultExecutor<StringResult>>();
      return executor.ExecuteAsync(context, this);
   }

   internal class Executor : ILambdaResultExecutor<StringResult>
   {
      private readonly ILambdaRuntime _lambdaRuntime;

      public Executor(ILambdaRuntime lambdaRuntime)
      {
         _lambdaRuntime = lambdaRuntime;
      }

      public async Task ExecuteAsync(ILambdaContext context, StringResult result)
      {
         using var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(result.Content));

         await _lambdaRuntime.ReplyWithInvocationSuccessAsync(outputStream, context);
      }
   }
}
