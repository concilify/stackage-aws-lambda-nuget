using System;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests
{
   public class StartupWithExceptionHandling : ILambdaStartup
   {
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddSingleton<ILambdaResultFactory, LambdaResultFactory>();
      }

      public void ConfigurePipeline(ILambdaPipelineBuilder pipelineBuilder)
      {
         pipelineBuilder.Use<ExceptionHandlingMiddleware>();
      }

      private class LambdaResultFactory : ILambdaResultFactory
      {
         public ILambdaResult UnhandledException(Exception exception)
         {
            return new StringResult($"An error occurred - {exception.Message}");
         }

         public ILambdaResult RemainingTimeExpired()
         {
            throw new NotSupportedException();
         }

         public ILambdaResult HostEndedRequest()
         {
            throw new NotSupportedException();
         }
      }
   }
}
