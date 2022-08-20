using System.Threading;
using System.Threading.Tasks;
using Stackage.Aws.Lambda.FakeRuntime.Model;

namespace Stackage.Aws.Lambda.FakeRuntime.Services
{
   public interface IFunctionsService
   {
      LambdaRequest Invoke(string functionName, string body);

      Task<LambdaRequest> WaitForNextInvocationAsync(string functionName, CancellationToken cancellationToken);

      void InvocationResponse(string functionName, string awsRequestId, string body);

      void InvocationError(string functionName, string awsRequestId, string body);

      void InitialisationError(string functionName, string body);

      LambdaCompletion GetCompletion(string functionName, string awsRequestId);
   }
}
