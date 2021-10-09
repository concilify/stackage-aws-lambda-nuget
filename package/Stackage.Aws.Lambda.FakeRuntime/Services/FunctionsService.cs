using System.Threading;
using System.Threading.Tasks;
using Stackage.Aws.Lambda.FakeRuntime.Model;

namespace Stackage.Aws.Lambda.FakeRuntime.Services
{
   public class FunctionsService : IFunctionsService
   {
      private readonly LambdaFunction.Dictionary _functions;
      private readonly IGenerateIds _idGenerator;

      public FunctionsService(
         LambdaFunction.Dictionary functions,
         IGenerateIds idGenerator)
      {
         _functions = functions;
         _idGenerator = idGenerator;
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
         return request;
      }

      public void InvocationResponse(string functionName, string awsRequestId, string body)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         function.Responses.TryAdd(awsRequestId, new LambdaResponse(awsRequestId, body, true));
      }

      public void InvocationError(string functionName, string awsRequestId, string body)
      {
         var function = _functions.GetOrAdd(functionName, name => new LambdaFunction(name));

         function.Responses.TryAdd(awsRequestId, new LambdaResponse(awsRequestId, body, false));
      }
   }
}
