using System;
using Amazon.Lambda.Core;
using FakeItEasy;

namespace Stackage.Aws.Lambda.Tests.Fakes
{
   public static class LambdaContextFake
   {
      public static ILambdaContext Valid() => With(remainingTime: TimeSpan.FromSeconds(30));

      public static ILambdaContext WithRemainingTime(TimeSpan remainingTime) => With(remainingTime: remainingTime);

      public static ILambdaContext With(
         string awsRequestId = "ValidRequestId",
         TimeSpan? remainingTime = null)
      {
         remainingTime ??= TimeSpan.FromMinutes(1);

         var context = A.Fake<ILambdaContext>();

         A.CallTo(() => context.AwsRequestId).Returns(awsRequestId);
         A.CallTo(() => context.RemainingTime).Returns(remainingTime.Value);

         return context;
      }
   }
}
