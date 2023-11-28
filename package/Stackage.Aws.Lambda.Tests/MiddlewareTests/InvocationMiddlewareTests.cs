using System;
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
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests.MiddlewareTests
{
   public class InvocationMiddlewareTests
   {
      [Test]
      public async Task returns_result_from_inner_delegate()
      {
         var expectedResult = A.Fake<ILambdaResult>();

         var middleware = CreateMiddleware();

         var pipelineDelegate = PipelineDelegateFake.Returns(expectedResult);

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            pipelineDelegate,
            CancellationToken.None);

         Assert.That(result, Is.SameAs(expectedResult));
      }

      [Test]
      public async Task cancellation_token_is_passed_to_inner_delegate()
      {
         var cancellationTokenSource = new CancellationTokenSource();

         var pipelineDelegate = PipelineDelegateFake.Callback(
            (_, _, _, requestAborted) =>
            {
               Assert.That(requestAborted.IsCancellationRequested, Is.False);
               cancellationTokenSource.Cancel();
               Assert.That(requestAborted.IsCancellationRequested, Is.True);
            });

         var middleware = CreateMiddleware();

         await middleware.InvokeAsync(
            new MemoryStream(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            pipelineDelegate,
            cancellationTokenSource.Token);
      }

      [Test]
      public async Task returns_error_result_when_inner_delegate_throws_exception()
      {
         var exceptionToThrow = new Exception();

         var middleware = CreateMiddleware();

         var pipelineDelegate = PipelineDelegateFake.Throws(exceptionToThrow);

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            pipelineDelegate,
            CancellationToken.None);

         Assert.That(result, Is.InstanceOf<ExceptionResult>());
         var exceptionResult = (ExceptionResult)result;
         Assert.That(exceptionResult.Exception, Is.SameAs(exceptionToThrow));
      }

      [Test]
      public async Task returns_cancellation_result_when_inner_delegate_throws_cancellation_exception_for_given_token()
      {
         var cancellationTokenSource = new CancellationTokenSource(0);
         var exceptionToThrow = new OperationCanceledException();

         var middleware = CreateMiddleware();

         var pipelineDelegate = PipelineDelegateFake.Throws(exceptionToThrow);

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            pipelineDelegate,
            cancellationTokenSource.Token);

         Assert.That(result, Is.InstanceOf<CancellationResult>());
         var cancellationResult = (CancellationResult)result;
         Assert.That(cancellationResult.Message, Is.EqualTo("The request was cancelled by the host; the handler may or may not have completed"));
      }

      [Test]
      public async Task returns_error_result_when_inner_delegate_throws_cancellation_exception_for_another_token()
      {
         var cancellationTokenSource = new CancellationTokenSource(10000);
         var exceptionToThrow = new OperationCanceledException();

         var middleware = CreateMiddleware();

         var pipelineDelegate = PipelineDelegateFake.Throws(exceptionToThrow);

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            pipelineDelegate,
            cancellationTokenSource.Token);

         Assert.That(result, Is.InstanceOf<ExceptionResult>());
         var exceptionResult = (ExceptionResult)result;
         Assert.That(exceptionResult.Exception, Is.SameAs(exceptionToThrow));
      }

      private static InvocationMiddleware CreateMiddleware(
         ILogger<InvocationMiddleware> logger = null)
      {
         logger ??= NullLogger<InvocationMiddleware>.Instance;

         return new InvocationMiddleware(logger);
      }
   }
}
