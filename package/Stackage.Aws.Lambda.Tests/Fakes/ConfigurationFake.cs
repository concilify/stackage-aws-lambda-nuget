using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public static class ConfigurationFake
{
   public static IConfiguration WithShutdownTimeoutMs(int shutdownTimeoutMs)
   {
      return new ConfigurationBuilder()
         .AddInMemoryCollection(new Dictionary<string, string>
         {
            ["ShutdownTimeoutMs"] = shutdownTimeoutMs.ToString()
         })
         .Build();
   }
}
