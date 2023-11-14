using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Stackage.Aws.Lambda.Abstractions;
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

         Task<ILambdaResult> ResolveWithinScopeTwice(
            StringPoco request,
            ILambdaContext context,
            IServiceProvider requestServices)
         {
            resolvedContracts.Add(requestServices.GetRequiredService<IContract>());
            resolvedContracts.Add(requestServices.GetRequiredService<IContract>());

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

      private static IRequestHandler<StringPoco> CreateHandler(
         ServiceCollection services = null,
         IRequestParser<StringPoco> parser = null,
         ILambdaSerializer serializer = null)
      {
         services ??= new ServiceCollection();
         parser ??= A.Fake<IRequestParser<StringPoco>>();
         serializer ??= A.Fake<ILambdaSerializer>();

         return new RequestHandler<StringPoco>(
            services.BuildServiceProvider(),
            parser,
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
