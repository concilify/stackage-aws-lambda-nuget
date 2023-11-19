using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Lambda.Middleware.Example.Integrations
{
   public class HttpObjectResult : ILambdaResult
   {
      private readonly object _object;
      private readonly int _statusCode;

      public HttpObjectResult(object @object, int statusCode = 200)
      {
         _object = @object;
         _statusCode = statusCode;
      }

      public Task ExecuteResultAsync(ILambdaContext context, IServiceProvider requestServices)
      {
         var executor = requestServices.GetRequiredService<ILambdaResultExecutor<HttpObjectResult>>();
         return executor.ExecuteAsync(context, this);
      }

      public class Executor : ILambdaResultExecutor<HttpObjectResult>
      {
         private readonly ILambdaSerializer _serializer;
         private readonly IRuntimeApiClient _runtimeApiClient;

         public Executor(
            ILambdaSerializer serializer,
            IRuntimeApiClient runtimeApiClient)
         {
            _serializer = serializer;
            _runtimeApiClient = runtimeApiClient;
         }

         public async Task ExecuteAsync(ILambdaContext context, HttpObjectResult result)
         {
            var responseObject = new
            {
               StatusCode = result._statusCode,
               Headers = new Dictionary<string, string>
               {
                  { "x-amzn-RequestId", context.AwsRequestId }
               },
               Body = result._object
            };

            using var response = new MemoryStream();

            _serializer.Serialize(responseObject, response);
            response.Position = 0;

            await _runtimeApiClient.SendResponseAsync(context.AwsRequestId, response, CancellationToken.None);
         }
      }
   }
}
