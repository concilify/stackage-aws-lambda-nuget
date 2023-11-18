using System;
using FakeItEasy;

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
}
