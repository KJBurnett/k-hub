namespace OpenLogi.Logging.Sinks;

/// <summary>Writes log entries to the console. Handy in development and the agent host.</summary>
public sealed class ConsoleLogSink : ILogSink
{
    private static readonly object Gate = new();

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        lock (Gate)
        {
            Console.WriteLine(entry.Format());
        }
    }
}
