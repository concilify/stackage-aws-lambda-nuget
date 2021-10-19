using System;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Extensions;
using Stackage.Aws.Lambda.Middleware;
using Stackage.Aws.Lambda.Results;

namespace Stackage.Aws.Lambda.Tests
{
   public class StartupWithExceptionHandling<TRequest> : ILambdaStartup<TRequest>
   {
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddSingleton<ILambdaResultFactory, LambdaResultFactory>();
      }

      public void ConfigurePipeline(ILambdaPipelineBuilder<TRequest> pipelineBuilder)
      {
         pipelineBuilder.Use<ExceptionHandlingMiddleware<TRequest>, TRequest>();
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
      }
   }
}
