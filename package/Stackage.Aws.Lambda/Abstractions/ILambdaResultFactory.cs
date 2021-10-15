using System;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaResultFactory
   {
      ILambdaResult UnhandledException(Exception exception, LambdaContext context);

      ILambdaResult RemainingTimeExpired(LambdaContext context);
   }
}
