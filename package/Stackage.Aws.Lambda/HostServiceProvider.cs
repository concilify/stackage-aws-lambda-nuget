using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Stackage.Aws.Lambda
{
   internal class HostServiceProvider : IServiceProvider
   {
      public HostServiceProvider()
      {
         HostEnvironment = CreateHostEnvironment();
         Configuration = CreateConfiguration(HostEnvironment);
      }

      public IHostEnvironment HostEnvironment { get; }

      public IConfiguration Configuration { get; }

      public object? GetService(Type serviceType)
      {
         if (serviceType == typeof(IHostEnvironment))
         {
            return HostEnvironment;
         }

         if (serviceType == typeof(IConfiguration))
         {
            return Configuration;
         }

         return null;
      }

      private static IHostEnvironment CreateHostEnvironment()
      {
         var environmentName = Environment.GetEnvironmentVariable($"DOTNET_{HostDefaults.EnvironmentKey}");
         var applicationName = Environment.GetEnvironmentVariable($"DOTNET_{HostDefaults.ApplicationKey}");
         var contentRootPath = Environment.GetEnvironmentVariable($"DOTNET_{HostDefaults.ContentRootKey}");

         var hostingEnvironment = new HostingEnvironment()
         {
            EnvironmentName = environmentName ?? Environments.Production,
            ApplicationName = applicationName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty,
            ContentRootPath = ResolveContentRootPath(contentRootPath, AppContext.BaseDirectory),
         };

         hostingEnvironment.ContentRootFileProvider = new PhysicalFileProvider(hostingEnvironment.ContentRootPath);

         return hostingEnvironment;
      }

      private static IConfiguration CreateConfiguration(IHostEnvironment hostEnvironment)
      {
         var builder = new ConfigurationBuilder();

         builder.SetBasePath(hostEnvironment.ContentRootPath);
         builder.AddJsonFile("appsettings.json", optional: true);

         if (hostEnvironment.EnvironmentName != null)
         {
            builder.AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true);
         }

         return builder.Build();
      }

      private static string ResolveContentRootPath(string? contentRootPath, string basePath)
      {
         if (string.IsNullOrEmpty(contentRootPath))
         {
            return basePath;
         }

         return Path.IsPathRooted(contentRootPath) ? contentRootPath : Path.Combine(Path.GetFullPath(basePath), contentRootPath);
      }

      private class HostingEnvironment : IHostEnvironment
      {
         public string EnvironmentName { get; set; } = string.Empty;

         public string ApplicationName { get; set; } = string.Empty;

         public string ContentRootPath { get; set; } = string.Empty;

         public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
      }
   }
}
