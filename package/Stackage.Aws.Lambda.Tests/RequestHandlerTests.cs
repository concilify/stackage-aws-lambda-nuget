using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Results;
using Stackage.Aws.Lambda.Tests.Fakes;
using Stackage.Aws.Lambda.Tests.Model;

namespace Stackage.Aws.Lambda.Tests
{
   public class RequestHandlerTests
   {
      [Test]
      public async Task each_request_is_handled_within_new_service_provider_scope()
      {
         var services = new ServiceCollection();
         services.AddScoped<IContract, Concrete>();

         var handler = CreateHandler(services);

         var resolvedContracts = new List<IContract>();

         Task<ILambdaResult> ResolveWithinScopeTwice(StringPoco request, LambdaContext context)
         {
            resolvedContracts.Add(context.RequestServices.GetRequiredService<IContract>());
            resolvedContracts.Add(context.RequestServices.GetRequiredService<IContract>());

            return Task.FromResult(A.Fake<ILambdaResult>());
         }

         await handler.HandleAsync(new MemoryStream(), LambdaContextFake.Valid(), ResolveWithinScopeTwice);
         await handler.HandleAsync(new MemoryStream(), LambdaContextFake.Valid(), ResolveWithinScopeTwice);

         // Resolutions within same request are same
         Assert.That(resolvedContracts[1], Is.SameAs(resolvedContracts[0]));
         Assert.That(resolvedContracts[3], Is.SameAs(resolvedContracts[2]));

         // Resolutions within different request are different
         Assert.That(resolvedContracts[0], Is.Not.SameAs(resolvedContracts[2]));
         Assert.That(resolvedContracts[1], Is.Not.SameAs(resolvedContracts[3]));
      }

      [Test]
      public async Task handler_is_cancelled_almost_immediately_when_remaining_time_is_fractionally_larger_than_shutdown_timeout()
      {
         var hostOptions = new HostOptions {ShutdownTimeout = TimeSpan.Zero};
         var lambdaResultFactory = LambdaResultFactoryFake.WithRemainingTimeExpiredResult("RemainingTimeExpired");

         var handler = CreateHandler(
            serializer: new CamelCaseLambdaJsonSerializer(),
            hostOptions: hostOptions,
            lambdaResultFactory: lambdaResultFactory);

         var stopwatch = Stopwatch.StartNew();

         var response = await handler.HandleAsync(new MemoryStream(), LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(1)), LongRunningTask);

         Assert.That(await response.ReadToEndAsync(), Is.EqualTo("RemainingTimeExpired"));
         Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(400));
      }

      [TestCase(0, 0)]
      [TestCase(50, 50)]
      [TestCase(1000, 1000)]
      [TestCase(50, 49)]
      [TestCase(50, 1)]
      [TestCase(50, -1)]
      public async Task handler_is_cancelled_almost_immediately_with_499_when_remaining_time_is_less_than_or_equal_to_shutdown_timeout(
         int shutdownMs,
         int remainingMs)
      {
         var hostOptions = new HostOptions {ShutdownTimeout = TimeSpan.FromMilliseconds(shutdownMs)};
         var lambdaResultFactory = LambdaResultFactoryFake.WithRemainingTimeExpiredResult("RemainingTimeExpired");

         var handler = CreateHandler(
            serializer: new CamelCaseLambdaJsonSerializer(),
            hostOptions: hostOptions,
            lambdaResultFactory: lambdaResultFactory);

         var stopwatch = Stopwatch.StartNew();

         var response = await handler.HandleAsync(
            new MemoryStream(),
            LambdaContextFake.WithRemainingTime(TimeSpan.FromMilliseconds(remainingMs)),
            LongRunningTask);

         Assert.That(await response.ReadToEndAsync(), Is.EqualTo("RemainingTimeExpired"));
         Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(400));
      }

      private static async Task<ILambdaResult> LongRunningTask(StringPoco request, LambdaContext context)
      {
         await Task.Delay(TimeSpan.FromHours(1), context.RequestAborted);

         return A.Fake<ILambdaResult>();
      }

      private static IRequestHandler<StringPoco> CreateHandler(
         ServiceCollection services = null,
         IRequestParser<StringPoco> parser = null,
         HostOptions hostOptions = null,
         ILambdaResultFactory lambdaResultFactory = null,
         ILambdaSerializer serializer = null)
      {
         services ??= new ServiceCollection();
         parser ??= A.Fake<IRequestParser<StringPoco>>();
         hostOptions ??= new HostOptions();
         lambdaResultFactory ??= new DefaultLambdaResultFactory();
         serializer ??= A.Fake<ILambdaSerializer>();

         var hostOptionsWrapper = A.Fake<IOptions<HostOptions>>();
         A.CallTo(() => hostOptionsWrapper.Value).Returns(hostOptions);

         return new RequestHandler<StringPoco>(
            services.BuildServiceProvider(),
            parser,
            hostOptionsWrapper,
            lambdaResultFactory,
            serializer);
      }

      private interface IContract
      {
      }

      private class Concrete : IContract
      {
      }
   }
}
