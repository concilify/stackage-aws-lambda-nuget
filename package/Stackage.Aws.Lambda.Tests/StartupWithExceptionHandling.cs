using Stackage.Aws.Lambda.Abstractions;
using Stackage.Aws.Lambda.Tests.Middleware;
using Stackage.Aws.Lambda.Extensions;

namespace Stackage.Aws.Lambda.Tests
{
   public class StartupWithExceptionHandling<TRequest> : IConfigurePipeline<TRequest>
   {
      public void ConfigurePipeline(ILambdaPipelineBuilder<TRequest> pipelineBuilder)
      {
         pipelineBuilder.Use<ExceptionHandlingMiddleware<TRequest>, TRequest>();
      }
   }
}
