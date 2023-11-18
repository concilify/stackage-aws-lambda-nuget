using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests;

public class LambdaListenerTests
{
   [Test]
   public async Task fff()
   {
      var cancellationToken = new CancellationToken(true);


      Assert.That(cancellationToken, Is.SameAs(cancellationToken));
   }

   [Test]
   public async Task non_cancelled_cancellation_token_is_passed_to_pipeline()
   {
      Task<ILambdaResult> CapturingPipelineDelegate(
         Stream stream, ILambdaContext context, IServiceProvider serviceProvider, CancellationToken cancellationToken)
      {
         Assert.That(cancellationToken.IsCancellationRequested, Is.False);

         throw new NotSupportedException();
      }

      var lambdaListener = CreateLambdaListener(
         pipelineAsync: CapturingPipelineDelegate);

      await lambdaListener.ListenAsync(new CancellationToken(false));
   }

   private static LambdaListener CreateLambdaListener(
      IRuntimeApiClient runtimeApiClient = null,
      IServiceProvider serviceProvider = null,
      PipelineDelegate pipelineAsync = null)
   {
      runtimeApiClient ??= RuntimeApiClientFake.Valid();
      serviceProvider ??= A.Fake<IServiceProvider>();
      pipelineAsync ??= (_, _, _, _) => Task.FromResult<ILambdaResult>(new StringResult("ValidResult"));

      return new LambdaListener(
         runtimeApiClient,
         serviceProvider,
         pipelineAsync,
         A.Fake<ILambdaSerializer>(),
         NullLogger<LambdaListener>.Instance);
   }
}
