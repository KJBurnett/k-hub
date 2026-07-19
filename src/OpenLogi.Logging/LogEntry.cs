namespace OpenLogi.Logging;

/// <summary>
/// A single immutable log record. Logs must contain enough context to reproduce
/// bugs without exposing user data (PLAN.md section 16, Appendix A #8), so
/// entries deliberately carry no personally identifying fields.
/// </summary>
public sealed record LogEntry(
    DateTimeOffset TimestampUtc,
    LogLevel Level,
    string Category,
    string Message,
    string? Exception = null)
{
    /// <summary>Formats the entry as a single stable log line.</summary>
    public string Format() =>
        $"{TimestampUtc:yyyy-MM-ddTHH:mm:ss.fffZ} [{Level,-11}] {Category}: {Message}"
        + (Exception is null ? string.Empty : $"{Environment.NewLine}{Exception}");
}
