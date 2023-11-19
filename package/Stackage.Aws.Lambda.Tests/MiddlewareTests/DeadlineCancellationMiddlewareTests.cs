using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

         var pipelineDelegate = PipelineDelegateFake.Returns(expectedResult);

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            LambdaContextFake.Valid(),
            A.Fake<IServiceProvider>(),
            pipelineDelegate);

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
         int shutdownTimeoutMs,
         int remainingMs)
      {
         var configuration = ConfigurationFake.WithShutdownTimeoutMs(shutdownTimeoutMs);
         var expectedResult = A.Fake<ILambdaResult>();
         var deadlineCancellation = new DeadlineCancellation();
         var resultFactory = LambdaResultFactoryFake.WithRemainingTimeExpiredResult(expectedResult);

         var middleware = CreateMiddleware(
            configuration: configuration,
            cancellationInitializer: deadlineCancellation,
            resultFactory: resultFactory);

         async Task<ILambdaResult> LongRunningInnerDelegate(
            Stream inputStream,
            ILambdaContext context,
            IServiceProvider requestServices,
            CancellationToken cancellationToken)
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

      [Test]
      public async Task cancellation_token_passed_to_inner_delegate_can_be_cancelled_by_incoming_cancellation_token()
      {
         var cancellationTokenSource = new CancellationTokenSource();

         var pipelineDelegate = PipelineDelegateFake.Callback(
            (_, _, _, cancellationToken) =>
            {
               Assert.That(cancellationToken.IsCancellationRequested, Is.False);
               cancellationTokenSource.Cancel();
               Assert.That(cancellationToken.IsCancellationRequested, Is.True);
            });

         var deadlineCancellation = new DeadlineCancellation();

         var middleware = CreateMiddleware(
            cancellationInitializer: deadlineCancellation);

         await middleware.InvokeAsync(
            new MemoryStream(),
            LambdaContextFake.WithRemainingTime(TimeSpan.FromSeconds(10)),
            A.Fake<IServiceProvider>(),
            pipelineDelegate,
            cancellationTokenSource.Token);

         Assert.That(deadlineCancellation.Token.IsCancellationRequested, Is.True);
         Assert.That(cancellationTokenSource.IsCancellationRequested, Is.True);
      }

      [Test]
      public async Task cancellation_token_passed_to_inner_delegate_can_be_cancelled_by_lack_of_time_remaining()
      {
         var cancellationTokenSource = new CancellationTokenSource();

         var pipelineDelegate = PipelineDelegateFake.AsyncCallback(
            async (_, _, _, cancellationToken) =>
            {
               Assert.That(cancellationToken.IsCancellationRequested, Is.False);

               await Task.Delay(10000, cancellationToken);

               Assert.That(cancellationToken.IsCancellationRequested, Is.True);
            });

         var configuration = ConfigurationFake.WithShutdownTimeoutMs(0);
         var deadlineCancellation = new DeadlineCancellation();

         var middleware = CreateMiddleware(
            configuration: configuration,
            cancellationInitializer: deadlineCancellation);

         await middleware.InvokeAsync(
            new MemoryStream(),
            LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
            A.Fake<IServiceProvider>(),
            pipelineDelegate,
            cancellationTokenSource.Token);

         Assert.That(deadlineCancellation.Token.IsCancellationRequested, Is.True);
         Assert.That(cancellationTokenSource.IsCancellationRequested, Is.False);
      }

      private static DeadlineCancellationMiddleware CreateMiddleware(
         IConfiguration configuration = null,
         IDeadlineCancellationInitializer cancellationInitializer = null,
         ILambdaResultFactory resultFactory = null,
         ILogger<DeadlineCancellationMiddleware> logger = null)
      {
         configuration ??= new ConfigurationBuilder().Build();
         cancellationInitializer ??= A.Fake<IDeadlineCancellationInitializer>();
         resultFactory ??= A.Fake<ILambdaResultFactory>();
         logger ??= NullLogger<DeadlineCancellationMiddleware>.Instance;

         return new DeadlineCancellationMiddleware(
            configuration,
            cancellationInitializer,
            resultFactory,
            logger);
      }
   }
}
