using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Stackage.Aws.Lambda
{
   internal class HostServiceProvider : IServiceProvider
   {
      private readonly HostBuilderContext _context;

      public HostServiceProvider(HostBuilderContext context)
      {
         _context = context;
      }

      public object? GetService(Type serviceType)
      {
         if (serviceType == typeof(IHostEnvironment))
         {
            return _context.HostingEnvironment;
         }

         if (serviceType == typeof(IConfiguration))
         {
            return _context.Configuration;
         }

         return null;
      }
   }
}
