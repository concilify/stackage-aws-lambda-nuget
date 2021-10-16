using System;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Integrations
{
   public class HttpLambdaResultFactory : ILambdaResultFactory
   {
      public ILambdaResult UnhandledException(Exception exception)
      {
         return new HttpErrorResult(500, "Internal Server Error");
      }
   }
}