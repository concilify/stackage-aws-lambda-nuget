using System;

namespace Stackage.Aws.Lambda.Options;

public class DeadlineCancellationOptions
{
   public TimeSpan HardInterval { get; set; } = TimeSpan.FromMilliseconds(100);

   public TimeSpan SoftInterval { get; set; } = TimeSpan.FromMilliseconds(1000);
}
