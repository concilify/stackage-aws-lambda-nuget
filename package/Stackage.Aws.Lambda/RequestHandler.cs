using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class RequestHandler<TRequest> : IRequestHandler<TRequest>
   {
      private readonly IServiceProvider _serviceProvider;
      private readonly IRequestParser<TRequest> _parser;
      private readonly HostOptions _hostOptions;
      private readonly ILambdaResultFactory _resultFactory;
      private readonly ILambdaSerializer _serializer;

      public RequestHandler(
         IServiceProvider serviceProvider,
         IRequestParser<TRequest> parser,
         IOptions<HostOptions> hostOptions,
         ILambdaResultFactory resultFactory,
         ILambdaSerializer serializer)
      {
         _serviceProvider = serviceProvider;
         _parser = parser;
         _hostOptions = hostOptions.Value;
         _resultFactory = resultFactory;
         _serializer = serializer;
      }

      public async Task<Stream> HandleAsync(
         Stream requestStream,
         ILambdaContext context,
         PipelineDelegate<TRequest> pipelineAsync)
      {
         var effectiveRemainingTimeMs = Math.Max((int) context.RemainingTime.Subtract(_hostOptions.ShutdownTimeout).TotalMilliseconds, 0);

         using var requestAborted = new CancellationTokenSource(effectiveRemainingTimeMs);
         using var scope = _serviceProvider.CreateScope();

         var wrapperContext = new DefaultLambdaContext(scope.ServiceProvider, requestAborted.Token, context);

         ILambdaResult lambdaResult;

         try
         {
            requestAborted.Token.ThrowIfCancellationRequested();

            lambdaResult = await pipelineAsync(
               _parser.Parse(requestStream),
               wrapperContext);
         }
         catch (OperationCanceledException) when (requestAborted.IsCancellationRequested)
         {
            lambdaResult = _resultFactory.RemainingTimeExpired();
         }

         return lambdaResult.SerializeResult(_serializer, wrapperContext);
      }
   }
}
