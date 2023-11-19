using System;
using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class ServiceProviderFake
{
   public static IServiceProvider Valid() => new ServiceCollection().BuildServiceProvider();

   public static IServiceProvider Returns<T>(T service)
      where T : class
   {
      var services = new ServiceCollection();

      services.AddSingleton(service);

      return services.BuildServiceProvider();
   }
}
