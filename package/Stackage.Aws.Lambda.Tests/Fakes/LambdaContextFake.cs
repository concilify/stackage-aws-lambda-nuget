using System;
using Amazon.Lambda.Core;
using FakeItEasy;

namespace Stackage.Aws.Lambda.Tests.Fakes
{
   public static class LambdaContextFake
   {
      public static ILambdaContext Valid()
      {
         var context = A.Fake<ILambdaContext>();

         A.CallTo(() => context.RemainingTime).Returns(TimeSpan.FromSeconds(30));

         return context;
      }

      public static ILambdaContext WithRemainingTime(TimeSpan remainingTime)
      {
         var context = A.Fake<ILambdaContext>();

         A.CallTo(() => context.RemainingTime).Returns(remainingTime);

         return context;
      }
   }
}
