using NUnit.Framework;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class StringResultTests
{
   [Test]
   public void content_is_accessible()
   {
      var testSubject = new StringResult("ArbitraryContent");

      Assert.That(testSubject.Content, Is.EqualTo("ArbitraryContent"));
   }
}
