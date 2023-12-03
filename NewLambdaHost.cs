using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class NewLambdaHost
   {
      private ILambdaSerializer _serializer = new CamelCaseLambdaJsonSerializer();
      private Func<ILambdaSerializer, LambdaBootstrapBuilder>? _bootstrapBuilderFactory;
      private IServiceProvider? _serviceProvider;

      private NewLambdaHost()
      {
      }

      public static NewLambdaHost Create<THandler, TRequest, TResponse>()
         where THandler : ILambdaHandler<TRequest, TResponse>
      {
         var host = new NewLambdaHost();

         host._bootstrapBuilderFactory = s => LambdaBootstrapBuilder.Create(
           (Func<TRequest, ILambdaContext, Task<TResponse>>)host.HandleAsync<THandler, TRequest, TResponse>, s);

         return host;
      }

      public static NewLambdaHost Create<THandler, TRequest>()
         where THandler : ILambdaHandler<TRequest>
      {
         var host = new NewLambdaHost();

         host._bootstrapBuilderFactory = s => LambdaBootstrapBuilder.Create(
            (Func<TRequest, ILambdaContext, Task>)host.HandleAsync<THandler, TRequest>, s);

         return host;
      }

      public NewLambdaHost UseSerializer(ILambdaSerializer serializer)
      {
         _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

         return this;
      }

      public NewLambdaHost UseMiddleware<TMiddleware, TRequest>()
         where TMiddleware : ILambdaMiddleware<TRequest>
      {


         return this;
      }

      public async Task RunAsync()
      {
         var bootstrap = _bootstrapBuilderFactory!(_serializer)
            .UseBootstrapHandler(InitialiseAsync)
            .Build();

         await bootstrap.RunAsync();
      }

      private static IServiceProvider BuildServiceProvider()
      {
         var services = new ServiceCollection();

         return services.BuildServiceProvider();
      }

      private Task<bool> InitialiseAsync()
      {
         _serviceProvider = BuildServiceProvider();

         // TODO: Build pipeline

         return Task.FromResult(true);
      }

      private Task HandleAsync<THandler, TRequest>(TRequest request, ILambdaContext context)
         where THandler : ILambdaHandler<TRequest>
      {
         using var scope = _serviceProvider!.CreateScope();

         var handler = ActivatorUtilities.CreateInstance<THandler>(scope.ServiceProvider);

         return handler.HandleAsync(request, context);
      }

      private Task<TResponse> HandleAsync<THandler, TRequest, TResponse>(TRequest request, ILambdaContext context)
         where THandler : ILambdaHandler<TRequest, TResponse>
      {
         using var scope = _serviceProvider!.CreateScope();

         var handler = ActivatorUtilities.CreateInstance<THandler>(scope.ServiceProvider);

         return handler.HandleAsync(request, context);
      }
   }
}
