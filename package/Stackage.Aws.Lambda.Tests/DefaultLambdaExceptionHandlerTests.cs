using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Middleware;

namespace Stackage.Aws.Lambda.Tests
{
   public class ExceptionHandlingMiddlewareTests
   {
      [Test]
      public void rethrows_exception_wrapped_in_unhandled_exception()
      {
         var exceptionToThrow = new Exception();

         var testSubject = CreateDefaultLambdaExceptionHandler();

         var exception = Assert.Throws<UnhandledException>(() => testSubject.HandleException(exceptionToThrow));

         Assert.That(exception!.InnerException, Is.SameAs(exceptionToThrow));
      }

      private static DefaultLambdaExceptionHandler CreateDefaultLambdaExceptionHandler(
         ILogger<DefaultLambdaExceptionHandler> logger = null)
      {
         logger ??= A.Fake<ILogger<DefaultLambdaExceptionHandler>>();

         return new DefaultLambdaExceptionHandler(
            logger);
      }
   }
}
