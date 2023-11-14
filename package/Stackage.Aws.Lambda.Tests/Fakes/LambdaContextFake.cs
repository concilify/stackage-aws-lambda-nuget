using System;
using Amazon.Lambda.Core;
using FakeItEasy;

namespace Stackage.Aws.Lambda.Tests.Fakes
{
   public static class LambdaContextFake
   {
      public static ILambdaContext Valid() => WithRemainingTime(TimeSpan.FromSeconds(30));

      public static ILambdaContext WithRemainingTime(TimeSpan remainingTime)
      {
         var context = A.Fake<ILambdaContext>();

         A.CallTo(() => context.RemainingTime).Returns(remainingTime);

         return context;
      }
   }
}
