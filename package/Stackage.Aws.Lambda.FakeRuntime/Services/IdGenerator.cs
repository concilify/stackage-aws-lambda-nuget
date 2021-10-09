using System;

namespace Stackage.Aws.Lambda.FakeRuntime.Services
{
   public class IdGenerator : IGenerateIds
   {
      public string Generate()
      {
         return Guid.NewGuid().ToString();
      }
   }
}
