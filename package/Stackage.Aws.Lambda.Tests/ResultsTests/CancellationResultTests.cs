using NUnit.Framework;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests.ResultsTests;

public class CancellationResultTests
{
   [Test]
   public void Message_is_accessible()
   {
      var testSubject = new CancellationResult("ArbitraryMessage");

      Assert.That(testSubject.Message, Is.EqualTo("ArbitraryMessage"));
   }
}
