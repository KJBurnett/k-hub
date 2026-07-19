namespace OpenLogi.Logging;

/// <summary>
/// The application logging facade. Every operation in OpenLogi should be
/// loggable (PLAN.md section 16). Loggers are cheap, injectable instances.
/// </summary>
public interface IAppLogger
{
    /// <summary>Returns true when entries at <paramref name="level"/> would be recorded.</summary>
    bool IsEnabled(LogLevel level);

    /// <summary>Writes a log entry at the given level.</summary>
    void Log(LogLevel level, string message, Exception? exception = null);
}

/// <summary>Convenience extensions over <see cref="IAppLogger"/>.</summary>
public static class AppLoggerExtensions
{
    /// <summary>Logs a trace-level message.</summary>
    public static void Trace(this IAppLogger logger, string message)
        => logger.Log(LogLevel.Trace, message);

    /// <summary>Logs a debug-level message.</summary>
    public static void Debug(this IAppLogger logger, string message)
        => logger.Log(LogLevel.Debug, message);

    /// <summary>Logs an information-level message.</summary>
    public static void Information(this IAppLogger logger, string message)
        => logger.Log(LogLevel.Information, message);

    /// <summary>Logs a warning-level message.</summary>
    public static void Warning(this IAppLogger logger, string message, Exception? exception = null)
        => logger.Log(LogLevel.Warning, message, exception);

    /// <summary>Logs an error-level message.</summary>
    public static void Error(this IAppLogger logger, string message, Exception? exception = null)
        => logger.Log(LogLevel.Error, message, exception);
}
