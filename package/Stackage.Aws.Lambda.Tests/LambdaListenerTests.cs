using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Tests.Fakes;

namespace Stackage.Aws.Lambda.Tests
{
   public class LambdaListenerTests
   {
      [Test]
      public async Task each_request_is_handled_within_new_service_provider_scope()
      {
         var cts = new CancellationTokenSource();

         var capturedServiceProviders = new List<IServiceProvider>();

         Task<Stream> CapturingPipelineDelegate(Stream r, ILambdaContext c, IServiceProvider serviceProvider)
         {
            capturedServiceProviders.Add(serviceProvider);

            if (capturedServiceProviders.Count >= 3)
            {
               cts.Cancel();
            }

            return Task.FromResult<Stream>(new MemoryStream());
         }

         var sp1 = A.Fake<IServiceProvider>();
         var sp2 = A.Fake<IServiceProvider>();
         var sp3 = A.Fake<IServiceProvider>();

         var lambdaListener = CreateLambdaListener(
            pipelineBuilder: LambdaPipelineBuilderFake.BuildReturns(CapturingPipelineDelegate),
            serviceProvider: ServiceProviderFake.CreateScopeReturns(sp1, sp2, sp3));

         await lambdaListener.ListenAsync(cts.Token);

         Assert.That(capturedServiceProviders.Count, Is.EqualTo(3));
         Assert.That(capturedServiceProviders[0], Is.SameAs(sp1));
         Assert.That(capturedServiceProviders[1], Is.SameAs(sp2));
         Assert.That(capturedServiceProviders[2], Is.SameAs(sp3));
      }

      private static ILambdaListener CreateLambdaListener(
         ILambdaPipelineBuilder pipelineBuilder = null,
         IRuntimeApiClient runtimeApiClient = null,
         IServiceProvider serviceProvider = null)
      {
         pipelineBuilder ??= A.Fake<ILambdaPipelineBuilder>();
         runtimeApiClient ??= RuntimeApiClientFake.Valid();
         serviceProvider ??= A.Fake<IServiceProvider>();

         return new LambdaListener(
            pipelineBuilder,
            runtimeApiClient,
            serviceProvider,
            A.Fake<IOptions<LambdaPipelineBuilderOptions>>(),
            A.Fake<ILogger<LambdaListener>>());
      }
   }
}
