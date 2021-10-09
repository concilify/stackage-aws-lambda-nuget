using System.IO;
using Amazon.Lambda.Core;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class RequestParser<TRequest> : IRequestParser<TRequest>
   {
      private readonly ILambdaSerializer _serializer;

      public RequestParser(ILambdaSerializer serializer)
      {
         _serializer = serializer;
      }

      public TRequest Parse(Stream stream)
      {
         var request = _serializer.Deserialize<TRequest>(stream);
         return request;
      }
   }
}
