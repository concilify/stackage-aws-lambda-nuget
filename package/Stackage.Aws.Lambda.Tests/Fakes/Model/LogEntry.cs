using System;
using Microsoft.Extensions.Logging;

namespace Stackage.Aws.Lambda.Tests.Fakes.Model;

public record LogEntry(string CategoryName, LogLevel LogLevel, string Message, Exception Exception);
