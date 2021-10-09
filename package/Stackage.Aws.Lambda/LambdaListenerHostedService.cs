using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Abstractions;

namespace Stackage.Aws.Lambda
{
   public class LambdaListenerHostedService<TRequest> : BackgroundService
   {
      private readonly ILambdaListener<TRequest> _listener;
      private readonly ILogger<LambdaListenerHostedService<TRequest>> _logger;

      public LambdaListenerHostedService(
         ILambdaListener<TRequest> listener,
         ILogger<LambdaListenerHostedService<TRequest>> logger)
      {
         _listener = listener;
         _logger = logger;
      }

      protected override async Task ExecuteAsync(CancellationToken cancellationToken)
      {
         _logger.LogDebug("Starting listener");

         await _listener.ListenAsync(cancellationToken);

         _logger.LogDebug("Stopping listener");
      }
   }
}
