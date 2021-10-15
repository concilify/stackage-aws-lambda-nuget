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

      public void Invoke(string functionName, string body)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         function.Requests.Enqueue(new LambdaRequest(_idGenerator.Generate(), body));
      }

      public async Task<LambdaRequest> WaitForNextInvocationAsync(string functionName, CancellationToken cancellationToken)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         var request = await function.Requests.DequeueAsync(cancellationToken);

         _logger.LogInformation("Invocation requested {awsRequestId} {request}", request.AwsRequestId, request.Body);

         return request;
      }

      public void InvocationResponse(string functionName, string awsRequestId, string body)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         function.Responses.TryAdd(awsRequestId, new LambdaResponse(awsRequestId, body, true));

         _logger.LogInformation("Invocation succeeded {awsRequestId} {response}", awsRequestId, body);
      }

      public void InvocationError(string functionName, string awsRequestId, string body)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         function.Responses.TryAdd(awsRequestId, new LambdaResponse(awsRequestId, body, false));

         _logger.LogInformation("Invocation failed {awsRequestId} {response}", awsRequestId, body);
      }
   }
}
