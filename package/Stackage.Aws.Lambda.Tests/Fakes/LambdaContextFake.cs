using System;
using Amazon.Lambda.Core;
using FakeItEasy;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Tests.Fakes
{
   public static class LambdaContextFake
   {
      public static LambdaContext Valid() => WithRemainingTime(TimeSpan.FromSeconds(30));

      public static LambdaContext WithRemainingTime(TimeSpan remainingTime)
      {
         var context = A.Fake<LambdaContext>();

         A.CallTo(() => context.RemainingTime).Returns(remainingTime);

         return context;
      }
   }
}
