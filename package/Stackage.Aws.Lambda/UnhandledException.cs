using System;

namespace Stackage.Aws.Lambda
{
   public class UnhandledException : Exception
   {
      public UnhandledException(string message, Exception innerException)
         : base(message, innerException)
      {
      }
   }
}
