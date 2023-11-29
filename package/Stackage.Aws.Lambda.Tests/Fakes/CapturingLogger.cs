using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Stackage.Aws.Lambda.Tests.Fakes.Model;

namespace Stackage.Aws.Lambda.Tests.Fakes;

public class CapturingLogger<T> : CapturingLogger, ILogger<T>
{
   public CapturingLogger(IList<LogEntry> logs)
      : base(typeof(T).Name, logs)
   {
   }
}

public class CapturingLogger : ILogger
{
   private readonly string _categoryName;
   private readonly IList<LogEntry> _logs;

   protected CapturingLogger(string categoryName, IList<LogEntry> logs)
   {
      _categoryName = categoryName;
      _logs = logs;
   }

   public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
   {
      _logs.Add(new LogEntry(_categoryName, logLevel, formatter(state, null), exception));
   }

   public bool IsEnabled(LogLevel logLevel)
   {
      return true;
   }

   public IDisposable BeginScope<TState>(TState state) where TState : notnull
   {
      return null;
   }

   public class Provider : ILoggerProvider
   {
      private readonly IList<LogEntry> _logs;

      public Provider(IList<LogEntry> logs)
      {
         _logs = logs ?? throw new ArgumentNullException(nameof(logs));
      }

      public ILogger CreateLogger(string categoryName)
      {
         return new CapturingLogger(categoryName, _logs);
      }

      public void Dispose()
      {
      }
   }
}
