using System;
using System.IO;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaExceptionHandler
   {
      Stream HandleException(Exception exception);
   }
}
