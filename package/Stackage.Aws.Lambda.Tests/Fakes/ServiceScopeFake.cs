using System;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class ServiceScopeFake
{
   public static IServiceScope Valid() => Contains(A.Fake<IServiceProvider>());

   private static IServiceScope Contains(IServiceProvider serviceProvider)
   {
      var serviceScope = A.Fake<IServiceScope>();

      A.CallTo(() => serviceScope.ServiceProvider).Returns(serviceProvider);

      return serviceScope;
   }
}
