namespace OpenLogi.Logging;

/// <summary>
/// A destination for log entries. Sinks are intentionally simple so new ones
/// (file, in-memory ring buffer for diagnostics, console) can be composed.
/// Implementations must be safe to call from multiple threads.
/// </summary>
public interface ILogSink
{
    /// <summary>Writes a single entry to the sink.</summary>
    void Write(LogEntry entry);
}
