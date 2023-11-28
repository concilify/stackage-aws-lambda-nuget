using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions;

public interface ILambdaRuntime
{
   Task<ILambdaInvocation> WaitForInvocationAsync(CancellationToken cancellationToken);

   Task ReplyWithInvocationSuccessAsync(Stream outputStream, ILambdaContext context);

   Task ReplyWithInvocationFailureAsync(Exception exception, ILambdaContext context);
}
