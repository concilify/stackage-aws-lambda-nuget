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

   private static IServiceProvider Configure(Action<ServiceCollection> callback)
   {
      var services = new ServiceCollection();

      callback(services);

      return services.BuildServiceProvider();
   }
}
