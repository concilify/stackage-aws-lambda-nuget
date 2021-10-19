using System;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class DefaultLambdaContext : LambdaContext
   {
      private readonly ILambdaContext _context;

      public DefaultLambdaContext(IServiceProvider requestServices, ILambdaContext context)
      {
         RequestServices = requestServices;
         _context = context;
      }

      public override IServiceProvider RequestServices { get; }

      public override string AwsRequestId => _context.AwsRequestId;

      public override IClientContext ClientContext => _context.ClientContext;

      public override string FunctionName => _context.FunctionName;

      public override string FunctionVersion => _context.FunctionVersion;

      public override ICognitoIdentity Identity => _context.Identity;

      public override string InvokedFunctionArn => _context.InvokedFunctionArn;

      public override ILambdaLogger Logger => _context.Logger;

      public override string LogGroupName => _context.LogGroupName;

      public override string LogStreamName => _context.LogStreamName;

      public override int MemoryLimitInMB => _context.MemoryLimitInMB;

      public override TimeSpan RemainingTime => _context.RemainingTime;
   }
}
