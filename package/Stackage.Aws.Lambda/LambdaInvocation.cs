using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda;

internal sealed record LambdaInvocation(Stream InputStream, ILambdaContext Context) : ILambdaInvocation
{
   public void Dispose() => InputStream.Dispose();
}
