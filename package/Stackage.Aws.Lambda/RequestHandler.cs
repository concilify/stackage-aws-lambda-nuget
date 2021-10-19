using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class RequestHandler<TRequest> : IRequestHandler<TRequest>
   {
      private readonly IServiceProvider _serviceProvider;
      private readonly IRequestParser<TRequest> _parser;
      private readonly ILambdaSerializer _serializer;

      public RequestHandler(
         IServiceProvider serviceProvider,
         IRequestParser<TRequest> parser,
         ILambdaSerializer serializer)
      {
         _serviceProvider = serviceProvider;
         _parser = parser;
         _serializer = serializer;
      }

      public async Task<Stream> HandleAsync(
         Stream requestStream,
         ILambdaContext context,
         PipelineDelegate<TRequest> pipelineAsync)
      {
         using var scope = _serviceProvider.CreateScope();

         var wrapperContext = new DefaultLambdaContext(scope.ServiceProvider, context);

         var lambdaResult = await pipelineAsync(
            _parser.Parse(requestStream),
            wrapperContext);

         return lambdaResult.SerializeResult(_serializer, wrapperContext);
      }
   }
}
