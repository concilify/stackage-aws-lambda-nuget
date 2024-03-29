using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results;

public class ObjectResult : ILambdaResult
{
   public ObjectResult(object @object)
   {
      Content = @object ?? throw new ArgumentNullException(nameof(@object));
   }

   public object Content { get; }

   public Task ExecuteResultAsync(ILambdaContext context, IServiceProvider requestServices)
   {
      var executor = requestServices.GetRequiredService<ILambdaResultExecutor<ObjectResult>>();
      return executor.ExecuteAsync(context, this);
   }

   internal class Executor : ILambdaResultExecutor<ObjectResult>
   {
      private readonly ILambdaSerializer _serializer;
      private readonly ILambdaRuntime _lambdaRuntime;

      public Executor(
         ILambdaSerializer serializer,
         ILambdaRuntime lambdaRuntime)
      {
         _serializer = serializer;
         _lambdaRuntime = lambdaRuntime;
      }

      public async Task ExecuteAsync(ILambdaContext context, ObjectResult result)
      {
         using var outputStream = new MemoryStream();

         _serializer.Serialize(result.Content, outputStream);
         outputStream.Position = 0;

         await _lambdaRuntime.ReplyWithInvocationSuccessAsync(outputStream, context);
      }
   }
}
