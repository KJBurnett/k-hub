namespace OpenLogi.Logging;

/// <summary>Creates <see cref="IAppLogger"/> instances bound to a category.</summary>
public interface IAppLoggerFactory
{
    /// <summary>Creates a logger for the given category name.</summary>
    IAppLogger CreateLogger(string category);

    /// <summary>Creates a logger categorised by the type <typeparamref name="T"/>.</summary>
    IAppLogger CreateLogger<T>() => CreateLogger(typeof(T).FullName ?? typeof(T).Name);
}

/// <summary>
/// Default logger factory. Fans entries out to a fixed set of sinks and filters
/// by a shared minimum level. Sinks are shared across all created loggers so a
/// single in-memory sink can back the diagnostics export (PLAN.md section 17).
/// </summary>
public sealed class AppLoggerFactory : IAppLoggerFactory
{
    private readonly IReadOnlyList<ILogSink> _sinks;
    private readonly LogLevel _minimumLevel;

    /// <summary>Creates a factory.</summary>
    /// <param name="minimumLevel">Lowest level that is recorded.</param>
    /// <param name="sinks">Destinations entries are written to.</param>
    public AppLoggerFactory(LogLevel minimumLevel, params ILogSink[] sinks)
    {
        ArgumentNullException.ThrowIfNull(sinks);
        _minimumLevel = minimumLevel;
        _sinks = sinks.ToArray();
    }

    /// <inheritdoc />
    public IAppLogger CreateLogger(string category)
        => new AppLogger(category, _minimumLevel, _sinks);

    private sealed class AppLogger : IAppLogger
    {
        private readonly string _category;
        private readonly LogLevel _minimumLevel;
        private readonly IReadOnlyList<ILogSink> _sinks;

        public AppLogger(string category, LogLevel minimumLevel, IReadOnlyList<ILogSink> sinks)
        {
            _category = category;
            _minimumLevel = minimumLevel;
            _sinks = sinks;
        }

        public bool IsEnabled(LogLevel level) => level >= _minimumLevel;

        public void Log(LogLevel level, string message, Exception? exception = null)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            var entry = new LogEntry(
                DateTimeOffset.UtcNow, level, _category, message, exception?.ToString());

            foreach (var sink in _sinks)
            {
                sink.Write(entry);
            }
        }
    }
}
