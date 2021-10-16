using System;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Integrations
{
   public class HttpApiV2LambdaResultFactory : ILambdaResultFactory
   {
      public ILambdaResult UnhandledException(Exception exception)
      {
         return new HttpApiV2ErrorResult(500, "Internal Server Error");
      }
   }
}
