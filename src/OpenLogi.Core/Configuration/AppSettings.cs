namespace OpenLogi.Core.Configuration;

/// <summary>
/// Application-wide settings. Offline-first and zero-telemetry are core,
/// non-negotiable values (PLAN.md sections 3 and 23): telemetry is intentionally
/// not a setting — it is always off — so it can never be turned on by accident
/// or by a future change.
/// </summary>
public sealed record AppSettings
{
    /// <summary>The default application settings.</summary>
    public static readonly AppSettings Default = new();

    /// <summary>Preferred UI theme.</summary>
    public AppTheme Theme { get; init; } = AppTheme.System;

    /// <summary>Whether the background agent starts automatically with the OS.</summary>
    public bool StartWithSystem { get; init; } = true;

    /// <summary>Whether closing the window minimises to the tray instead of exiting.</summary>
    public bool MinimizeToTray { get; init; } = true;

    /// <summary>Minimum severity that is written to the log.</summary>
    public LogVerbosity LogVerbosity { get; init; } = LogVerbosity.Information;

    /// <summary>
    /// Telemetry is always disabled and cannot be enabled. Exposed as a
    /// read-only property so the guarantee is explicit in the model.
    /// </summary>
    public bool TelemetryEnabled => false;
}

/// <summary>Log verbosity levels selectable by the user (PLAN.md section 16).</summary>
public enum LogVerbosity
{
    /// <summary>Only warnings and errors.</summary>
    Warning = 0,

    /// <summary>Normal operational logging (default).</summary>
    Information,

    /// <summary>Verbose diagnostic logging for troubleshooting.</summary>
    Debug,
}
