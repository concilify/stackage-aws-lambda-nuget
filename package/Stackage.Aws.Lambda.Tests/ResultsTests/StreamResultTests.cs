using System.IO;
using NUnit.Framework;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class StreamResultTests
{
   [Test]
   public void Content_is_accessible()
   {
      var content = new MemoryStream();
      var testSubject = new StreamResult(content);

      Assert.That(testSubject.Content, Is.SameAs(content));
   }
}
