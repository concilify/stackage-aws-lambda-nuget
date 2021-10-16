using System;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Results
{
   public class DefaultLambdaResultFactory : ILambdaResultFactory
   {
      public ILambdaResult UnhandledException(Exception exception)
      {
         return new StringResult("Internal Server Error");
      }

      public ILambdaResult RemainingTimeExpired()
      {
         return new StringResult("Client Closed Request");
      }
   }
}
