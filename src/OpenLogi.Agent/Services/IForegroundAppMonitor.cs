namespace OpenLogi.Agent.Services;

/// <summary>Event data carrying the foreground executable name (without path).</summary>
public sealed class ForegroundAppChangedEventArgs : EventArgs
{
    /// <summary>Creates the event args.</summary>
    public ForegroundAppChangedEventArgs(string? executableName) => ExecutableName = executableName;

    /// <summary>The foreground executable file name, e.g. "chrome.exe", or null if unknown.</summary>
    public string? ExecutableName { get; }
}

/// <summary>
/// Reports the currently focused application so the agent can switch profiles
/// automatically (PLAN.md sections 9 and 12). The real Windows implementation
/// (Win32 foreground-window polling) arrives in Phase 4; this abstraction keeps
/// the switching logic testable and platform independent.
/// </summary>
public interface IForegroundAppMonitor
{
    /// <summary>The executable name currently in the foreground, or null if unknown.</summary>
    string? CurrentExecutable { get; }

    /// <summary>Raised when the foreground application changes.</summary>
    event EventHandler<ForegroundAppChangedEventArgs>? ForegroundChanged;

    /// <summary>Starts monitoring.</summary>
    void Start();

    /// <summary>Stops monitoring.</summary>
    void Stop();
}
