using System;
using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class ServiceProviderFake
{
   public static IServiceProvider Valid() => new ServiceCollection().BuildServiceProvider();

   public static IServiceProvider Returns<T>(T service)
      where T : class
   {
      return Configure(services => services.AddSingleton(service));
   }

   public static IServiceProvider Returns<T1, T2>(T1 service1, T2 service2)
      where T1 : class
      where T2 : class
   {
      return Configure(services =>
      {
         services.AddSingleton(service1);
         services.AddSingleton(service2);
      });
   }

   private static IServiceProvider Configure(Action<ServiceCollection> callback)
   {
      var services = new ServiceCollection();

      callback(services);

      return services.BuildServiceProvider();
   }
}
