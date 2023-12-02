using NUnit.Framework;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class ObjectResultTests
{
   [Test]
   public void content_is_accessible()
   {
      var content = new object();
      var testSubject = new ObjectResult(content);

      Assert.That(testSubject.Content, Is.SameAs(content));
   }
}
