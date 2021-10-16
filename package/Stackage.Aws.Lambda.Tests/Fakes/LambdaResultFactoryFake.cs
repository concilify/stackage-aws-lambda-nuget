using FakeItEasy;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.Fakes
{
   public static class LambdaResultFactoryFake
   {
      public static ILambdaResultFactory WithRemainingTimeExpiredResult(string message)
      {
         var context = A.Fake<ILambdaResultFactory>();

         A.CallTo(() => context.RemainingTimeExpired()).Returns(new StringResult(message));

         return context;
      }
   }
}
