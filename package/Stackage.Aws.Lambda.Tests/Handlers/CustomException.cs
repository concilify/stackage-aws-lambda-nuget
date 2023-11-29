using System;

namespace Stackage.Aws.Lambda.Tests.Handlers;

public class CustomException : Exception
{
   public CustomException(string message) : base(message)
   {
   }
}
