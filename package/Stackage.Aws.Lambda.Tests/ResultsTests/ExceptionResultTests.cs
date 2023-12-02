using System;
using NUnit.Framework;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class ExceptionResultTests
{
   [Test]
   public void Exception_is_accessible()
   {
      var exception = new Exception();
      var testSubject = new ExceptionResult(exception);

      Assert.That(testSubject.Exception, Is.SameAs(exception));
   }
}
