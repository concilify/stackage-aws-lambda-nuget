using System.IO;
using Amazon.Lambda.Core;

namespace Stackage.Aws.Lambda.Abstractions
{
   public interface ILambdaResult
   {
      Stream SerializeResult(ILambdaSerializer serializer, ILambdaContext context);
   }
}
