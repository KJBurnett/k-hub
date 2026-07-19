using System.Collections.Concurrent;

namespace OpenLogi.Logging.Sinks;

/// <summary>
/// Keeps the most recent log entries in a bounded in-memory ring buffer. This
/// backs the diagnostics export (PLAN.md section 17) without ever touching the
/// network or writing user data off the machine.
/// </summary>
public sealed class InMemoryLogSink : ILogSink
{
    private readonly ConcurrentQueue<LogEntry> _entries = new();
    private readonly int _capacity;

    /// <summary>Creates the sink.</summary>
    /// <param name="capacity">Maximum number of entries to retain.</param>
    /// <exception cref="ArgumentOutOfRangeException">When capacity is not positive.</exception>
    public InMemoryLogSink(int capacity = 2000)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be positive.");
        }

        _capacity = capacity;
    }

    /// <inheritdoc />
    public void Write(LogEntry entry)
    {
        _entries.Enqueue(entry);
        while (_entries.Count > _capacity && _entries.TryDequeue(out _))
        {
            // Trim oldest entries to stay within capacity.
        }
    }

    /// <summary>Returns a snapshot of the currently retained entries, oldest first.</summary>
    public IReadOnlyList<LogEntry> Snapshot() => _entries.ToArray();
}
