using System;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaResultFactory
   {
      ILambdaResult UnhandledException(Exception exception);

      ILambdaResult RemainingTimeExpired();

      ILambdaResult HostEndedRequest();
   }
}
