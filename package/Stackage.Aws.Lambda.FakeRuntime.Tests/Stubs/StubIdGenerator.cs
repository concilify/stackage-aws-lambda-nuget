using System.Collections.Generic;
using Stackage.Aws.Lambda.FakeRuntime.Services;

namespace Stackage.Aws.Lambda.FakeRuntime.Tests.Stubs
{
   public class StubIdGenerator : IGenerateIds
   {
      private readonly Queue<string> _ids;

      public StubIdGenerator(params string[] ids)
      {
         _ids = new Queue<string>(ids);
      }

      public string Generate()
      {
         return _ids.Dequeue();
      }
   }
}
