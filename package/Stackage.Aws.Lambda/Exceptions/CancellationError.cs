using System;

namespace Stackage.Aws.Lambda.Exceptions;

internal class CancellationError : Exception
{
   public CancellationError(string message) : base(message)
   {
   }
}
