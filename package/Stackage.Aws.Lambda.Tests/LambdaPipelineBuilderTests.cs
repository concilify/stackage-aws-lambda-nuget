using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
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
         (_, _, cancellationToken) =>
         {
            Assert.That(cancellationToken.IsCancellationRequested, Is.False);
            cancellationTokenSource.Cancel();
            Assert.That(cancellationToken.IsCancellationRequested, Is.True);
         });

      await pipelineAsync(
         new MemoryStream(),
         LambdaContextFake.Valid(),
         ServiceProviderFake.Returns(handlerExecutor),
         cancellationTokenSource.Token);
   }
}
