using System;

namespace Stackage.Aws.Lambda.Exceptions;

internal class InitialisationError : Exception
{
   public InitialisationError(string message) : base(message)
   {
   }
}
