namespace OpenLogi.Logging.Sinks;

/// <summary>
/// Appends log entries to a UTF-8 text file. Writes are serialised so the sink
/// is safe to share across threads. Failures to write are swallowed so that
/// logging never crashes the application.
/// </summary>
public sealed class FileLogSink : ILogSink, IDisposable
{
    private readonly object _gate = new();
    private readonly StreamWriter _writer;

    /// <summary>Creates the sink, ensuring the parent directory exists.</summary>
    /// <param name="filePath">Path to the log file. It is created if missing and appended to otherwise.</param>
    public FileLogSink(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _writer = new StreamWriter(filePath, append: true) { AutoFlush = true };
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        lock (_gate)
        {
            try
            {
                _writer.WriteLine(entry.Format());
            }
            catch (IOException)
            {
                // Logging must never take down the app; drop the entry on disk errors.
            }
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            _writer.Dispose();
        }
    }
}
