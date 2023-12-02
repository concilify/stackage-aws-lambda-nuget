using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda;

internal class LambdaRuntime : ILambdaRuntime
{
   private readonly IRuntimeApiClient _runtimeApiClient;

   public LambdaRuntime(IRuntimeApiClient runtimeApiClient)
   {
      _runtimeApiClient = runtimeApiClient;
   }

   public async Task<ILambdaInvocation> WaitForInvocationAsync(CancellationToken cancellationToken)
   {
      var invocationRequest = await _runtimeApiClient.GetNextInvocationAsync(cancellationToken);

      return new LambdaInvocation(invocationRequest.InputStream, invocationRequest.LambdaContext);
   }

   public async Task ReplyWithInvocationSuccessAsync(Stream? outputStream, ILambdaContext context)
   {
      await _runtimeApiClient.SendResponseAsync(context.AwsRequestId, outputStream);
   }

   public async Task ReplyWithInvocationFailureAsync(Exception exception, ILambdaContext context)
   {
      await _runtimeApiClient.ReportInvocationErrorAsync(context.AwsRequestId, exception);
   }
}
