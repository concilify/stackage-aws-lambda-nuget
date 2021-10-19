using System;
using FakeItEasy;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Tests.Fakes
{
   public static class LambdaResultFactoryFake
   {
      public static ILambdaResultFactory WithUnhandledExceptionResult(Exception exception, ILambdaResult result)
      {
         var resultFactory = A.Fake<ILambdaResultFactory>();

         A.CallTo(() => resultFactory.UnhandledException(exception)).Returns(result);

         return resultFactory;
      }

      public static ILambdaResultFactory WithRemainingTimeExpiredResult(ILambdaResult result)
      {
         var resultFactory = A.Fake<ILambdaResultFactory>();

         A.CallTo(() => resultFactory.RemainingTimeExpired()).Returns(result);

         return resultFactory;
      }
   }
}
