using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class ServiceScopeFactoryFake
{
   public static IServiceScopeFactory Valid() => CreateScopeReturns(ServiceScopeFake.Valid());

   private static IServiceScopeFactory CreateScopeReturns(IServiceScope serviceScope)
   {
      var serviceScopeFactory = A.Fake<IServiceScopeFactory>();

      A.CallTo(() => serviceScopeFactory.CreateScope()).Returns(serviceScope);

      return serviceScopeFactory;
   }
}
