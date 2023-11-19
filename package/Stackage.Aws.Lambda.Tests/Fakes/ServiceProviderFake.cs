using System;
using System.Linq;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class ServiceProviderFake
{
   public static IServiceProvider Valid() => Returns(ServiceScopeFactoryFake.Valid());

   public static IServiceProvider Returns<T>(T service)
   {
      var serviceProvider = A.Fake<IServiceProvider>();

      A.CallTo(() => serviceProvider.GetService(typeof(T)))
         .Returns(service);

      return serviceProvider;
   }

   public static IServiceProvider CreateScopeReturns(params IServiceProvider[] serviceProviders)
   {
      var results = serviceProviders
         .Select(sp => ServiceScopeFactoryFake.CreateScopeReturns(ServiceScopeFake.Contains(sp)))
         .Cast<object>()
         .ToArray();

      var serviceProvider = A.Fake<IServiceProvider>();

      A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory)))
         .ReturnsNextFromSequence(results);

      return serviceProvider;
   }
}
