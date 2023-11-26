using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests;

public class LambdaListenerTests
{
   [Test]
   public async Task each_request_is_handled_within_new_service_provider_scope()
   {
      var cts = new CancellationTokenSource();

      var capturedServiceProviders = new List<IServiceProvider>();

      Task<ILambdaResult> CapturingPipelineDelegate(
         Stream stream, ILambdaContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken)
      {
         capturedServiceProviders.Add(serviceProvider);

         if (capturedServiceProviders.Count >= 3)
         {
            cts.Cancel();
         }

         return Task.FromResult<ILambdaResult>(new StringResult("Arbitrary Result"));
      }

      var lambdaListener = CreateLambdaListener(
         pipelineAsync: CapturingPipelineDelegate);

      await lambdaListener.ListenAsync(cts.Token);

      Assert.That(capturedServiceProviders.Count, Is.EqualTo(3));
      Assert.That(capturedServiceProviders.All(sp => sp != null), Is.True);
      Assert.That(capturedServiceProviders[0], Is.Not.SameAs(capturedServiceProviders[1]));
      Assert.That(capturedServiceProviders[0], Is.Not.SameAs(capturedServiceProviders[2]));
      Assert.That(capturedServiceProviders[1], Is.Not.SameAs(capturedServiceProviders[2]));
   }

   [Test]
   public async Task cancellation_token_is_passed_to_wait_for_invocation()
   {
      var cancellationTokenSource = new CancellationTokenSource();

      var lambdaRuntime = LambdaRuntimeFake.WaitForInvocationCallback(
         cancellationToken =>
         {
            Assert.That(cancellationToken.IsCancellationRequested, Is.False);
            cancellationTokenSource.Cancel();
            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
         });

      var lambdaListener = CreateLambdaListener(
         lambdaRuntime: lambdaRuntime);

      await lambdaListener.ListenAsync(cancellationTokenSource.Token);
   }

   [Test]
   public async Task cancellation_token_is_passed_to_pipeline()
   {
      var cancellationTokenSource = new CancellationTokenSource();

      var pipelineAsync = PipelineDelegateFake.Callback(
         (_, _, _, requestAborted) =>
         {
            Assert.That(requestAborted.IsCancellationRequested, Is.False);
            cancellationTokenSource.Cancel();
            Assert.That(requestAborted.IsCancellationRequested, Is.True);
         });

      var lambdaListener = CreateLambdaListener(
         pipelineAsync: pipelineAsync);

      await lambdaListener.ListenAsync(cancellationTokenSource.Token);
   }

   [Test]
   public async Task invokes_execute_of_lambda_result_returned_from_pipeline()
   {
      var cancellationTokenSource = new CancellationTokenSource();
      var lambdaResult = A.Fake<ILambdaResult>();

      var context = LambdaContextFake.Valid();
      var pipelineAsync = PipelineDelegateFake.Returns(lambdaResult);

      var lambdaListener = CreateLambdaListener(
         pipelineAsync: pipelineAsync);

      await lambdaListener.InvokeAndReplyAsync(new LambdaInvocation(new MemoryStream(), context), cancellationTokenSource.Token);

      A.CallTo(() => lambdaResult.ExecuteResultAsync(context, A<IServiceProvider>._))
         .MustHaveHappenedOnceExactly();
   }

   [Test]
   public async Task invokes_execute_of_cancellation_result_when_pipeline_is_cancelled_for_given_token()
   {
      var cancellationTokenSource = new CancellationTokenSource(0);
      var exceptionToThrow = new OperationCanceledException();
      var lambdaResultExecutor = A.Fake<ILambdaResultExecutor<CancellationResult>>();

      var context = LambdaContextFake.Valid();
      var pipelineAsync = PipelineDelegateFake.Throws(exceptionToThrow);

      var lambdaListener = CreateLambdaListener(
         serviceProvider: ServiceProviderFake.Returns(lambdaResultExecutor),
         pipelineAsync: pipelineAsync);

      await lambdaListener.InvokeAndReplyAsync(new LambdaInvocation(new MemoryStream(), context), cancellationTokenSource.Token);

      A.CallTo(() => lambdaResultExecutor.ExecuteAsync(context, A<CancellationResult>._))
         .MustHaveHappenedOnceExactly();
   }

   [Test]
   public async Task invokes_execute_of_exception_result_when_pipeline_is_cancelled_for_another_token()
   {
      var cancellationTokenSource = new CancellationTokenSource(10000);
      var exceptionToThrow = new OperationCanceledException();
      var lambdaResultExecutor = A.Fake<ILambdaResultExecutor<ExceptionResult>>();

      var context = LambdaContextFake.Valid();
      var pipelineAsync = PipelineDelegateFake.Throws(exceptionToThrow);

      var lambdaListener = CreateLambdaListener(
         serviceProvider: ServiceProviderFake.Returns(lambdaResultExecutor),
         pipelineAsync: pipelineAsync);

      await lambdaListener.InvokeAndReplyAsync(new LambdaInvocation(new MemoryStream(), context), cancellationTokenSource.Token);

      A.CallTo(() => lambdaResultExecutor.ExecuteAsync(
            context,
            A<ExceptionResult>.That.Matches(r => r.Exception == exceptionToThrow)))
         .MustHaveHappenedOnceExactly();
   }

   private static LambdaListener CreateLambdaListener(
      ILambdaRuntime lambdaRuntime = null,
      IServiceProvider serviceProvider = null,
      PipelineDelegate pipelineAsync = null)
   {
      lambdaRuntime ??= LambdaRuntimeFake.Valid();
      serviceProvider ??= ServiceProviderFake.Returns(A.Fake<ILambdaResultExecutor<StringResult>>());
      pipelineAsync ??= PipelineDelegateFake.Valid();

      return new LambdaListener(
         lambdaRuntime,
         serviceProvider,
         pipelineAsync,
         NullLogger<LambdaListener>.Instance);
   }
}
