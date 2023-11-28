using System.Diagnostics.CodeAnalysis;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda.Extensions
{
   public static class LambdaPipelineBuilderExtensions
   {
      public static ILambdaPipelineBuilder Use<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMiddleware>(
         this ILambdaPipelineBuilder pipelineBuilder)
         where TMiddleware : ILambdaMiddleware
      {
         return pipelineBuilder.Use(MiddlewareExtensions.Resolve<TMiddleware>());
      }
   }
}
