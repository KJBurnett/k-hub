namespace OpenLogi.Logging;

/// <summary>Severity of a log entry, ordered from least to most severe.</summary>
public enum LogLevel
{
    /// <summary>Highly detailed tracing, normally disabled.</summary>
    Trace = 0,

    /// <summary>Diagnostic detail useful when troubleshooting.</summary>
    Debug,

    /// <summary>Normal operational events.</summary>
    Information,

    /// <summary>Unexpected but recoverable situations.</summary>
    Warning,

    /// <summary>Failures that require attention.</summary>
    Error,
}
