using System;
using System.IO;
using System.Text;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results
{
   public class DefaultLambdaResultFactory : ILambdaResultFactory
   {
      public ILambdaResult UnhandledException(Exception exception, LambdaContext context)
      {
         return new StringResult("Internal Server Error");
      }

      public ILambdaResult RemainingTimeExpired(LambdaContext context)
      {
         return new StringResult("Client Closed Request");
      }
   }
}
