using System;
using System.IO;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions;

public interface ILambdaInvocation : IDisposable
{
   Stream InputStream { get; }

   ILambdaContext Context { get; }
}
