using System.Threading;
using System.Threading.Tasks;
using Stackage.Aws.Lambda.FakeRuntime.Model;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.Tests
{
   public class CancelOnResponseOrErrorFunctionsService : IFunctionsService
   {
      private readonly IFunctionsService _decorated;
      private readonly CancellationTokenSource _cancellationTokenSource;

      public CancelOnResponseOrErrorFunctionsService(
         IFunctionsService functionsService,
         CancellationTokenSource cancellationTokenSource)
      {
         _decorated = functionsService;
         _cancellationTokenSource = cancellationTokenSource;
      }

      public void Invoke(string functionName, string body)
      {
         _decorated.Invoke(functionName, body);
      }

      public async Task<LambdaRequest> WaitForNextInvocationAsync(string functionName, CancellationToken cancellationToken)
      {
         return await _decorated.WaitForNextInvocationAsync(functionName, cancellationToken);
      }

      public void InvocationResponse(string functionName, string awsRequestId, string body)
      {
         _decorated.InvocationResponse(functionName, awsRequestId, body);
         _cancellationTokenSource.Cancel();
      }

      public void InvocationError(string functionName, string awsRequestId, string body)
      {
         _decorated.InvocationError(functionName, awsRequestId, body);
         _cancellationTokenSource.Cancel();
      }
   }
}
