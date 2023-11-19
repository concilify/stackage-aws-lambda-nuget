using System;
using System.Collections.Generic;
using System.IO;
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

      var sp1 = A.Fake<IServiceProvider>();
      var sp2 = A.Fake<IServiceProvider>();
      var sp3 = A.Fake<IServiceProvider>();

      var lambdaListener = CreateLambdaListener(
         pipelineAsync: CapturingPipelineDelegate,
         serviceProvider: ServiceProviderFake.CreateScopeReturns(sp1, sp2, sp3));

      await lambdaListener.ListenAsync(cts.Token);

      Assert.That(capturedServiceProviders.Count, Is.EqualTo(3));
      Assert.That(capturedServiceProviders[0], Is.SameAs(sp1));
      Assert.That(capturedServiceProviders[1], Is.SameAs(sp2));
      Assert.That(capturedServiceProviders[2], Is.SameAs(sp3));
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
         (_, _, _, cancellationToken) =>
         {
            Assert.That(cancellationToken.IsCancellationRequested, Is.False);
            cancellationTokenSource.Cancel();
            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
         });

      var lambdaListener = CreateLambdaListener(
         pipelineAsync: pipelineAsync);

      await lambdaListener.ListenAsync(cancellationTokenSource.Token);
   }

   [Test]
   public async Task cancellation_token_is_passed_to_reply_with_success()
   {
      var cancellationTokenSource = new CancellationTokenSource();

      var lambdaRuntime = LambdaRuntimeFake.ReplyWithInvocationSuccessCallback(
         (_, _, cancellationToken) =>
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
   public async Task cancellation_token_is_passed_to_reply_with_failure()
   {
      var exceptionToThrow = new Exception();
      var cancellationTokenSource = new CancellationTokenSource();

      var lambdaRuntime = LambdaRuntimeFake.ReplyWithInvocationFailureCallback(
         (exception, _, cancellationToken) =>
         {
            Assert.That(exception, Is.SameAs(exceptionToThrow));
            Assert.That(cancellationToken.IsCancellationRequested, Is.False);
            cancellationTokenSource.Cancel();
            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
         });

      var lambdaListener = CreateLambdaListener(
         lambdaRuntime: lambdaRuntime,
         pipelineAsync: PipelineDelegateFake.Throws(exceptionToThrow));

      await lambdaListener.ListenAsync(cancellationTokenSource.Token);
   }

   private static LambdaListener CreateLambdaListener(
      ILambdaRuntime lambdaRuntime = null,
      IServiceProvider serviceProvider = null,
      PipelineDelegate pipelineAsync = null)
   {
      lambdaRuntime ??= LambdaRuntimeFake.Valid();
      serviceProvider ??= ServiceProviderFake.Valid();
      pipelineAsync ??= PipelineDelegateFake.Valid();

      return new LambdaListener(
         lambdaRuntime,
         serviceProvider,
         pipelineAsync,
         A.Fake<ILambdaSerializer>(),
         NullLogger<LambdaListener>.Instance);
   }
}
