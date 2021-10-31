using System;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.LambdaPackage
{
   public class HttpLambdaResultFactory : ILambdaResultFactory
   {
      public ILambdaResult UnhandledException(Exception exception)
      {
         // TODO: Replace with ContentResult anonymous overload
         return new StringResult("Internal Server Error");
      }

      public ILambdaResult RemainingTimeExpired()
      {
         // TODO: Replace with ContentResult anonymous overload
         return new StringResult("Client Closed Request");
      }
   }
}
