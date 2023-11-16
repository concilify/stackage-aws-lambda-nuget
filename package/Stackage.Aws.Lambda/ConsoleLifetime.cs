using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Stackage.Aws.Lambda;

public sealed class ConsoleLifetime : IDisposable
{
   private readonly CancellationTokenSource _cancellationTokenSource = new();
   private readonly PosixSignalRegistration _sigintListener;
   private readonly PosixSignalRegistration _sigtermListener;

   public ConsoleLifetime()
   {
      Console.WriteLine("Application started. Press Ctrl+C to shut down.");

      _sigintListener = PosixSignalRegistration.Create(PosixSignal.SIGINT, HandlePosixSignal);
      _sigtermListener = PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandlePosixSignal);
   }

   public CancellationToken Token => _cancellationTokenSource.Token;

   private void HandlePosixSignal(PosixSignalContext context)
   {
      Console.WriteLine($"Application received {context.Signal}. Shutting down...");

      _cancellationTokenSource.Cancel();

      context.Cancel = true;
   }

   public void Dispose()
   {
      _sigintListener.Dispose();
      _sigtermListener.Dispose();

      Console.WriteLine("Application shut down.");
   }
}
