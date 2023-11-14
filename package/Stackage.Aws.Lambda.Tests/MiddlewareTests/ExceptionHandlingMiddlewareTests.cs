using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Tests.Fakes;
using Stackage.Aws.Lambda.Tests.Model;

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
            StringPoco request,
            ILambdaContext context,
            IServiceProvider requestServices)
         {
            return Task.FromResult(expectedResult);
         }

         var result = await middleware.InvokeAsync(
            new StringPoco(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            InnerDelegate);

         Assert.That(result, Is.SameAs(expectedResult));
      }

      [Test]
      public async Task returns_error_result_when_inner_delegate_throws_exception()
      {
         var exceptionToThrow = new Exception();
         var expectedResult = A.Fake<ILambdaResult>();

         var resultFactory = LambdaResultFactoryFake.WithUnhandledExceptionResult(exceptionToThrow, expectedResult);

         var middleware = CreateMiddleware(resultFactory: resultFactory);

         Task<ILambdaResult> InnerDelegate(
            StringPoco request,
            ILambdaContext context,
            IServiceProvider requestServices)
         {
            throw exceptionToThrow;
         }

         var result = await middleware.InvokeAsync(
            new StringPoco(),
            A.Fake<ILambdaContext>(),
            A.Fake<IServiceProvider>(),
            InnerDelegate);

         Assert.That(result, Is.SameAs(expectedResult));
      }

      private static ILambdaMiddleware<StringPoco> CreateMiddleware(
         ILambdaResultFactory resultFactory = null,
         ILogger<ExceptionHandlingMiddleware<StringPoco>> logger = null)
      {
         resultFactory ??= A.Fake<ILambdaResultFactory>();
         logger ??= A.Fake<ILogger<ExceptionHandlingMiddleware<StringPoco>>>();

         return new ExceptionHandlingMiddleware<StringPoco>(
            resultFactory,
            logger);
      }
   }
}
