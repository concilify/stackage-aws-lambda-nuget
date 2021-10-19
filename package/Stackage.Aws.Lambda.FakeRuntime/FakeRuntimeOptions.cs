using System;

namespace Stackage.Aws.Lambda.FakeRuntime
{
   public class FakeRuntimeOptions
   {
      public TimeSpan DeadlineTimeout { get; set; } = TimeSpan.FromSeconds(30);
   }
}
