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
   private readonly string _content;

   public StringResult(string content)
   {
      _content = content ?? throw new ArgumentNullException(nameof(content));
   }

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
         using var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(result._content));

         await _lambdaRuntime.ReplyWithInvocationSuccessAsync(outputStream, context);
      }
   }
}
