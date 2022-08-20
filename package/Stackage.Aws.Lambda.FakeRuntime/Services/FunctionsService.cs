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

         _logger.LogInformation(
            "Function {functionName} invoked {awsRequestId} {request}",
            functionName, request.AwsRequestId, request.Body);

         return request;
      }

      public async Task<LambdaRequest> WaitForNextInvocationAsync(string functionName, CancellationToken cancellationToken)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         _logger.LogInformation(
            "Function {functionName} waiting for invocation...",
            functionName);

         try
         {
            var request = await function.QueuedRequests.DequeueAsync(cancellationToken);

            function.InFlightRequests.TryAdd(request.AwsRequestId, request);

            _logger.LogInformation(
               "Function {functionName} scheduled {awsRequestId}",
               functionName, request.AwsRequestId);

            return request;
         }
         catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
         {
            _functions.TryRemove(functionName, out _);

            _logger.LogInformation(
               "Function {functionName} disconnected",
               functionName);
            throw;
         }
      }

      public void InvocationResponse(string functionName, string awsRequestId, string body)
      {
         RequestCompleted(functionName, awsRequestId, body, true);

         _logger.LogInformation(
            "Function {functionName} succeeded {awsRequestId} {response}",
            functionName, awsRequestId, body);
      }

      public void InvocationError(string functionName, string awsRequestId, string body)
      {
         RequestCompleted(functionName, awsRequestId, body, false);

         _logger.LogInformation(
            "Function {functionName} failed {awsRequestId} {response}",
            functionName, awsRequestId, body);
      }

      public void InitialisationError(string functionName, string body)
      {
         throw new NotSupportedException();
         // TODO:
         // Non-recoverable initialization error. Runtime should exit after reporting
         //    the error. Error will be served in response to the first invoke.

         _functions.TryRemove(functionName, out _);

         _logger.LogInformation(
            "Function {functionName} failed initialisation {response}",
            functionName, body);
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
