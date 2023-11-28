using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests;

public class LambdaPipelineBuilderTests
{
   [Test]
   public async Task cancellation_token_is_passed_to_handler_executor()
   {
      var cancellationTokenSource = new CancellationTokenSource();
      var builder = new LambdaPipelineBuilder();

      var pipelineAsync = builder.Build();

      var handlerExecutor = LambdaHandlerExecutorFake.ExecuteCallback(
         (_, _, requestAborted) =>
         {
            Assert.That(requestAborted.IsCancellationRequested, Is.False);
            cancellationTokenSource.Cancel();
            Assert.That(requestAborted.IsCancellationRequested, Is.True);
         });

      await pipelineAsync(
         new MemoryStream(),
         LambdaContextFake.Valid(),
         ServiceProviderFake.Returns(
            handlerExecutor,
            (ILogger<InvocationMiddleware>)NullLogger<InvocationMiddleware>.Instance),
         cancellationTokenSource.Token);
   }
}
