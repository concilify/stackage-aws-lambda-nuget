using System;

namespace Stackage.Aws.Lambda.Exceptions;

internal class UnhandledError : Exception
{
   public UnhandledError(string message) : base(message)
   {
   }
}
