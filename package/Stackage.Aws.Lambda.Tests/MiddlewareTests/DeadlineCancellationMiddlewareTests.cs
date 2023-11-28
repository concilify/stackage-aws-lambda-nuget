using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Options;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;
using Microsoft.Extensions.Options;
using NUnit.Framework.Constraints;

namespace Stackage.Aws.Lambda.Tests.MiddlewareTests
{
   public class DeadlineCancellationMiddlewareTests
   {
      [TestCaseSource(nameof(NonCancellationTestCases))]
      public async Task invoke_returns_pipeline_result(
         PipelineDelegate pipelineDelegate,
         ILambdaContext context,
         DeadlineCancellationOptions options,
         ILambdaResult expectedLambdaResult,
         Constraint elapsedMsConstraint)
      {
         var deadlineCancellation = new DeadlineCancellation();

         var (result, elapsedMs) = await InvokeAsync(
            pipelineDelegate: pipelineDelegate,
            context: context,
            options: options,
            cancellationInitializer: deadlineCancellation);

         Assert.That(result, Is.SameAs(expectedLambdaResult));
         Assert.That(deadlineCancellation.Token.IsCancellationRequested, Is.False);
         Assert.That(elapsedMs, elapsedMsConstraint);
      }

      [TestCaseSource(nameof(CancellationTestCases))]
      public async Task invoke_returns_cancellation_result(
         PipelineDelegate pipelineDelegate,
         ILambdaContext context,
         DeadlineCancellationOptions options,
         string expectedCancellationResultMessage,
         bool expectedIsCancellationRequested,
         Constraint elapsedMsConstraint)
      {
         var deadlineCancellation = new DeadlineCancellation();

         var (result, elapsedMs) = await InvokeAsync(
            pipelineDelegate: pipelineDelegate,
            context: context,
            options: options,
            cancellationInitializer: deadlineCancellation);

         Assert.That(result, Is.InstanceOf<CancellationResult>());
         var cancellationResult = (CancellationResult) result;
         Assert.That(cancellationResult.Message, Is.EqualTo(expectedCancellationResultMessage));
         Assert.That(deadlineCancellation.Token.IsCancellationRequested, Is.EqualTo(expectedIsCancellationRequested));
         Assert.That(elapsedMs, elapsedMsConstraint);
      }

      [Test]
      public void bubbles_exception_to_caller_when_inner_delegate_cancelled_by_incoming_cancellation_token()
      {
         var cancellationTokenSource = new CancellationTokenSource(200);

         Assert.ThrowsAsync<TaskCanceledException>(async () =>
         {
            await InvokeAsync(
               PipelineDelegateFake.LongRunningAndExpectsToBeCancelled(),
               LambdaContextFake.WithRemainingTime(TimeSpan.FromSeconds(10)),
               options: CreateDeadlineCancellationOptions(1, 10),
               requestAborted: cancellationTokenSource.Token);
         });
      }

      [Test]
      public void bubbles_exception_to_caller_when_inner_delegate_cancelled_by_another_cancellation_token()
      {
         var exceptionToThrow = new OperationCanceledException();

         var exception = Assert.ThrowsAsync<OperationCanceledException>(async () =>
         {
            await InvokeAsync(
               PipelineDelegateFake.Throws(exceptionToThrow),
               LambdaContextFake.WithRemainingTime(TimeSpan.FromSeconds(10)),
               options: CreateDeadlineCancellationOptions(1, 10));
         });

         Assert.That(exception, Is.SameAs(exceptionToThrow));
      }

      [Test]
      public void bubbles_exception_to_caller_when_inner_delegate_throws_after_cancellation()
      {
         var exceptionToThrow = new Exception();

         var exception = Assert.ThrowsAsync<Exception>(async () =>
         {
            await InvokeAsync(
               PipelineDelegateFake.ThrowsAfterCancellation(exceptionToThrow),
               LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
               options: CreateDeadlineCancellationOptions(0, 100));
         });

         Assert.That(exception, Is.SameAs(exceptionToThrow));
      }

      private static TestCaseData[] NonCancellationTestCases()
      {
         var lambdaResult = A.Fake<ILambdaResult>();

         return new[]
         {
            new TestCaseData(
                  PipelineDelegateFake.Returns(lambdaResult, latencyMs: 70),
                  LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
                  CreateDeadlineCancellationOptions(30, 70),
                  lambdaResult,
                  Is.LessThan(100)) // Middleware should run for c.70ms
               .SetName("Request completes before hard and soft intervals expire"),
            new TestCaseData(
                  PipelineDelegateFake.Returns(lambdaResult, latencyMs: 70),
                  LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
                  CreateDeadlineCancellationOptions(0, 100),
                  lambdaResult,
                  Is.LessThan(120)) // Middleware should run for c.70ms
               .SetName("Request completes before soft interval expires"),
            new TestCaseData(
                  PipelineDelegateFake.Returns(lambdaResult, latencyMs: 70),
                  LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
                  CreateDeadlineCancellationOptions(100, 0),
                  lambdaResult,
                  Is.LessThan(120)) // Middleware should run for c.70ms
               .SetName("Request completes before hard interval expires")
         };
      }

      private static TestCaseData[] CancellationTestCases()
      {
         const string shortcutCancellationMessage =
            "The request was shortcut due to lack of remaining time; the handler was not invoked";
         const string hardCancellationMessage =
            "The request was cancelled due to lack of remaining time; the handler failed to respond and may not have completed";
         const string softCancellationMessage =
            "The request was cancelled due to lack of remaining time; the handler responded promptly but may not have completed";

         return new[]
         {
            new TestCaseData(
                  PipelineDelegateFake.Throws(),
                  LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
                  CreateDeadlineCancellationOptions(100, 101),
                  shortcutCancellationMessage,
                  false,
                  Is.LessThan(20)) // Middleware should run for hardly any time due to shortcut
               .SetName("Request is shortcut when combined hard and soft intervals greater than remaining time"),
            new TestCaseData(
                  PipelineDelegateFake.Throws(),
                  LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
                  CreateDeadlineCancellationOptions(0, 201),
                  shortcutCancellationMessage,
                  false,
                  Is.LessThan(20)) // Middleware should run for hardly any time due to shortcut
               .SetName("Request is shortcut when combined soft interval alone is greater than remaining time"),
            new TestCaseData(
                  PipelineDelegateFake.Throws(),
                  LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
                  CreateDeadlineCancellationOptions(201, 0),
                  shortcutCancellationMessage,
                  false,
                  Is.LessThan(20)) // Middleware should run for hardly any time due to shortcut
               .SetName("Request is shortcut when combined hard interval alone is greater than remaining time"),
            new TestCaseData(
                  PipelineDelegateFake.LongRunningAndExpectsToBeCancelled(),
                  LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
                  CreateDeadlineCancellationOptions(100, 90),
                  softCancellationMessage,
                  true,
                  Is.LessThan(40)) // Middleware should run for c.10ms (remainingTime - hardTimeout - softTimeout)
               .SetName("Request is cancelled gracefully when inner pipeline responds to cancellation token"),
            new TestCaseData(
                  PipelineDelegateFake.LongRunningIgnoresCancellationToken(),
                  LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(200)),
                  CreateDeadlineCancellationOptions(100, 90),
                  hardCancellationMessage,
                  true,
                  Is.LessThan(130)) // Middleware should run for c.100ms (remainingTime - hardTimeout)
               .SetName("Request is cancelled forcibly when inner pipeline ignores to cancellation token"),
         };
      }

      private static async Task<(ILambdaResult LambdaResult, long ElapsedMs)> InvokeAsync(
         PipelineDelegate pipelineDelegate,
         ILambdaContext context,
         DeadlineCancellationOptions options = null,
         IDeadlineCancellationInitializer cancellationInitializer = null,
         CancellationToken requestAborted = default)
      {
         var middleware = CreateMiddleware(
            options: options,
            cancellationInitializer: cancellationInitializer);

         var stopwatch = Stopwatch.StartNew();

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            context,
            A.Fake<IServiceProvider>(),
            pipelineDelegate,
            requestAborted);

         return (result, stopwatch.ElapsedMilliseconds);
      }

      private static DeadlineCancellationOptions CreateDeadlineCancellationOptions(int hardIntervalMs, int softIntervalMs)
      {
         return new DeadlineCancellationOptions
         {
            HardInterval = TimeSpan.FromMilliseconds(hardIntervalMs),
            SoftInterval = TimeSpan.FromMilliseconds(softIntervalMs),
         };
      }

      private static DeadlineCancellationMiddleware CreateMiddleware(
         IDeadlineCancellationInitializer cancellationInitializer = null,
         DeadlineCancellationOptions options = null,
         ILogger<DeadlineCancellationMiddleware> logger = null)
      {
         cancellationInitializer ??= A.Fake<IDeadlineCancellationInitializer>();
         options ??= new DeadlineCancellationOptions();
         logger ??= NullLogger<DeadlineCancellationMiddleware>.Instance;

         return new DeadlineCancellationMiddleware(
            cancellationInitializer,
            new OptionsWrapper<DeadlineCancellationOptions>(options),
            logger);
      }
   }
}
