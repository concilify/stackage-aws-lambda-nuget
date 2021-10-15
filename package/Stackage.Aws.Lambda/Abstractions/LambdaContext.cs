using System;
using System.Threading;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public abstract class LambdaContext : ILambdaContext
   {
      public abstract IServiceProvider RequestServices { get; }

      public abstract CancellationToken RequestAborted { get; }

      public abstract string AwsRequestId { get; }

      public abstract IClientContext ClientContext { get; }

      public abstract string FunctionName { get; }

      public abstract string FunctionVersion { get; }

      public abstract ICognitoIdentity Identity { get; }

      public abstract string InvokedFunctionArn { get; }

      public abstract ILambdaLogger Logger { get; }

      public abstract string LogGroupName { get; }

      public abstract string LogStreamName { get; }

      public abstract int MemoryLimitInMB { get; }

      public abstract TimeSpan RemainingTime { get; }
   }
}
