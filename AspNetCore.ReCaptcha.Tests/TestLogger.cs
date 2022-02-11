using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AspNetCore.ReCaptcha.Tests;

public class TestLogger<T> : ILogger<T>
    where T : class
{
    public List<(LogLevel LogLevel, string Message)> LogEntries { get; } = new List<(LogLevel, string)>();

    public IDisposable BeginScope<TState>(TState state)
    {
        return new NullDisposable();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogEntries.Add((logLevel, formatter(state, exception)));
    }
}

internal class NullDisposable : IDisposable
{
    public void Dispose()
    {
    }
}
