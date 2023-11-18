using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests.MiddlewareTests
{
   public class ExceptionHandlingMiddlewareTests
   {
      [Test]
      public async Task returns_result_from_inner_delegate()
      {
         var expectedResult = A.Fake<ILambdaResult>();

         var middleware = CreateMiddleware();

         Task<ILambdaResult> InnerDelegate(
            Stream request,
            ILambdaContext context,
            IServiceProvider requestServices,
            CancellationToken cancellationToken)
         {
            return Task.FromResult(expectedResult);
         }

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            InnerDelegate);

         Assert.That(result, Is.SameAs(expectedResult));
      }

      [Test]
      public void cancellation_token_is_passed_to_inner_delegate()
      {
         Assert.Fail();
      }

      [Test]
      public async Task returns_error_result_when_inner_delegate_throws_exception()
      {
         var exceptionToThrow = new Exception();
         var expectedResult = A.Fake<ILambdaResult>();

         var resultFactory = LambdaResultFactoryFake.WithUnhandledExceptionResult(exceptionToThrow, expectedResult);

         var middleware = CreateMiddleware(resultFactory: resultFactory);

         Task<ILambdaResult> InnerDelegate(
            Stream request,
            ILambdaContext context,
            IServiceProvider requestServices,
            CancellationToken cancellationToken)
         {
            throw exceptionToThrow;
         }

         var result = await middleware.InvokeAsync(
            new MemoryStream(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            InnerDelegate);

         Assert.That(result, Is.SameAs(expectedResult));
      }

      private static ILambdaMiddleware CreateMiddleware(
         ILambdaResultFactory resultFactory = null,
         ILogger<ExceptionHandlingMiddleware> logger = null)
      {
         resultFactory ??= A.Fake<ILambdaResultFactory>();
         logger ??= A.Fake<ILogger<ExceptionHandlingMiddleware>>();

         return new ExceptionHandlingMiddleware(
            resultFactory,
            logger);
      }
   }
}
