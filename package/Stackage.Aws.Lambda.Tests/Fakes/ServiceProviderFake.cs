using System;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class ServiceProviderFake
{
   public static IServiceProvider Valid()
   {
      var serviceProvider = A.Fake<IServiceProvider>();

      A.CallTo(() => serviceProvider.GetService(typeof(IServiceScopeFactory)))
         .Returns(ServiceScopeFactoryFake.Valid());

      return serviceProvider;
   }
}
