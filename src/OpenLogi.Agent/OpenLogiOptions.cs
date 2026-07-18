using OpenLogi.Logging;

namespace OpenLogi.Agent;

/// <summary>
/// Options controlling how the OpenLogi services are composed. Defaults are
/// chosen for a lightweight, offline-first desktop install (PLAN.md sections 3,
/// 15, 21).
/// </summary>
public sealed class OpenLogiOptions
{
    /// <summary>
    /// Directory for the local database and logs. Defaults to
    /// <c>%LOCALAPPDATA%/OpenLogi</c> (or the platform equivalent).
    /// </summary>
    public string DataDirectory { get; set; } = DefaultDataDirectory();

    /// <summary>Minimum log level written to the sinks.</summary>
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// When true, a mock device (and a sample application rule) is seeded so the
    /// UI and agent can be exercised with no physical hardware. This keeps the
    /// software useful on CI and on unsupported platforms during early phases.
    /// </summary>
    public bool UseDemoDevices { get; set; }

    /// <summary>The path to the local SQLite database file.</summary>
    public string DatabasePath => Path.Combine(DataDirectory, "openlogi.sqlite");

    /// <summary>The path to the rolling log file.</summary>
    public string LogFilePath => Path.Combine(DataDirectory, "logs", "openlogi.log");

    private static string DefaultDataDirectory()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrEmpty(root))
        {
            root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".openlogi");
        }

        return Path.Combine(root, "OpenLogi");
    }
}
