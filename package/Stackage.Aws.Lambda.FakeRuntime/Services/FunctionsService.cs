using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.FakeRuntime.Model;

namespace Stackage.Aws.Lambda.FakeRuntime.Services
{
   public class FunctionsService : IFunctionsService
   {
      private readonly LambdaFunction.Dictionary _functions;
      private readonly IGenerateIds _idGenerator;
      private readonly ILogger<FunctionsService> _logger;

      public FunctionsService(
         LambdaFunction.Dictionary functions,
         IGenerateIds idGenerator,
         ILogger<FunctionsService> logger)
      {
         _functions = functions;
         _idGenerator = idGenerator;
         _logger = logger;
      }

      public LambdaRequest Invoke(string functionName, string body)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         var request = new LambdaRequest(_idGenerator.Generate(), body);

         function.QueuedRequests.Enqueue(request);

         _logger.LogInformation("Invocation requested {awsRequestId} {request}", request.AwsRequestId, request.Body);

         return request;
      }

      public async Task<LambdaRequest> WaitForNextInvocationAsync(string functionName, CancellationToken cancellationToken)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         var request = await function.QueuedRequests.DequeueAsync(cancellationToken);

         function.InFlightRequests.TryAdd(request.AwsRequestId, request);

         _logger.LogInformation("Invocation scheduled {awsRequestId} {request}", request.AwsRequestId, request.Body);

         return request;
      }

      public void InvocationResponse(string functionName, string awsRequestId, string body)
      {
         RequestCompleted(functionName, awsRequestId, body, true);

         _logger.LogInformation("Invocation succeeded {awsRequestId} {response}", awsRequestId, body);
      }

      public void InvocationError(string functionName, string awsRequestId, string body)
      {
         RequestCompleted(functionName, awsRequestId, body, false);

         _logger.LogInformation("Invocation failed {awsRequestId} {response}", awsRequestId, body);
      }

      public LambdaCompletion GetCompletion(string functionName, string awsRequestId)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         if (!function.CompletedRequests.TryGetValue(awsRequestId, out var completion))
         {
            throw new InvalidOperationException("Unrecognised Amazon Request Id");
         }

         return completion;
      }

      private void RequestCompleted(string functionName, string awsRequestId, string responseBody, bool success)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         if (!function.InFlightRequests.TryRemove(awsRequestId, out var outboxRequest))
         {
            throw new InvalidOperationException("Unrecognised Amazon Request Id");
         }

         function.CompletedRequests.TryAdd(awsRequestId, new LambdaCompletion(awsRequestId, outboxRequest.Body, responseBody, success));

         outboxRequest.NotifyCompletion();
      }
   }
}
