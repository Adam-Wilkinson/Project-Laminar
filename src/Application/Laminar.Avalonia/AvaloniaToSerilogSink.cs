using System;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;

namespace Laminar.Avalonia;

public class AvaloniaToSerilogSink(ILogger<App> logger) : ILogSink
{
    public bool IsEnabled(LogEventLevel level, string area)
    {
        throw new System.NotImplementedException();
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate)
    {
        logger.Log(level.ToMicrosoft, messageTemplate);
    }

    public void Log(LogEventLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        logger.Log(level.ToMicrosoft, messageTemplate, propertyValues);
    }
}

public static class AvaloniaLogEventLevelExtensions
{
    extension(LogEventLevel level)
    {
        public LogLevel ToMicrosoft => level switch
        {
            LogEventLevel.Verbose => LogLevel.Trace,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => LogLevel.None,
        };
    }
}