using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests.MiddlewareTests
{
   public class DeadlineCancellationMiddlewareTests
   {
      [Test]
      public async Task returns_result_from_inner_delegate()
      {
         var expectedResult = A.Fake<ILambdaResult>();

         var middleware = CreateMiddleware();

         Task<ILambdaResult> InnerDelegate(
            Stream request,
            ILambdaContext context,
            IServiceProvider requestServices)
         {
            return Task.FromResult(expectedResult);
         }

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            LambdaContextFake.Valid(),
            A.Fake<IServiceProvider>(),
            InnerDelegate);

         Assert.That(result, Is.SameAs(expectedResult));
      }

      // when_remaining_time_is_fractionally_larger_than_shutdown_timeout
      [TestCase(0, 10)]
      [TestCase(1000, 1010)]
      // when_remaining_time_is_less_than_or_equal_to_shutdown_timeout
      [TestCase(0, 0)]
      [TestCase(50, 50)]
      [TestCase(1000, 1000)]
      [TestCase(50, 49)]
      [TestCase(50, 1)]
      [TestCase(50, -1)]
      public static async Task invoke_is_cancelled_almost_immediately_and_returns_remaining_time_result(
         int shutdownMs,
         int remainingMs)
      {
         var hostOptions = new HostOptions {ShutdownTimeout = TimeSpan.FromMilliseconds(shutdownMs)};
         var expectedResult = A.Fake<ILambdaResult>();
         var deadlineCancellation = new DeadlineCancellation();
         var resultFactory = LambdaResultFactoryFake.WithRemainingTimeExpiredResult(expectedResult);

         var middleware = CreateMiddleware(
            hostOptions: hostOptions,
            cancellationInitializer: deadlineCancellation,
            resultFactory: resultFactory);

         async Task<ILambdaResult> LongRunningInnerDelegate(
            Stream request,
            ILambdaContext context,
            IServiceProvider requestServices)
         {
            await Task.Delay(TimeSpan.FromHours(1), deadlineCancellation.Token);

            return new StringResult("Should be cancelled before getting this far");
         }

         var stopwatch = Stopwatch.StartNew();

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(remainingMs)),
            A.Fake<IServiceProvider>(),
            LongRunningInnerDelegate);

         Assert.That(result, Is.SameAs(expectedResult));
         Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(400));
      }

      private static ILambdaMiddleware CreateMiddleware(
         HostOptions hostOptions = null,
         IDeadlineCancellationInitializer cancellationInitializer = null,
         ILambdaResultFactory resultFactory = null,
         ILogger<DeadlineCancellationMiddleware> logger = null)
      {
         hostOptions ??= new HostOptions();
         cancellationInitializer ??= A.Fake<IDeadlineCancellationInitializer>();
         resultFactory ??= A.Fake<ILambdaResultFactory>();
         logger ??= A.Fake<ILogger<DeadlineCancellationMiddleware>>();

         var hostOptionsWrapper = A.Fake<IOptions<HostOptions>>();
         A.CallTo(() => hostOptionsWrapper.Value).Returns(hostOptions);

         return new DeadlineCancellationMiddleware(
            hostOptionsWrapper,
            cancellationInitializer,
            resultFactory,
            logger);
      }
   }
}
