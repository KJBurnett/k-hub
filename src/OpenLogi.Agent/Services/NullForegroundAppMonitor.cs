namespace OpenLogi.Agent.Services;

/// <summary>
/// A no-op foreground monitor used until the platform-specific implementation
/// lands in Phase 4. It never reports a foreground application, so the agent
/// falls back to the default profile. It can also be driven manually in tests
/// via <see cref="SetForeground"/>.
/// </summary>
public sealed class NullForegroundAppMonitor : IForegroundAppMonitor
{
    /// <inheritdoc />
    public string? CurrentExecutable { get; private set; }

    /// <inheritdoc />
    public event EventHandler<ForegroundAppChangedEventArgs>? ForegroundChanged;

    /// <inheritdoc />
    public void Start()
    {
    }

    /// <inheritdoc />
    public void Stop()
    {
    }

    /// <summary>Simulates a foreground change (used by tests and demo mode).</summary>
    public void SetForeground(string? executableName)
    {
        CurrentExecutable = executableName;
        ForegroundChanged?.Invoke(this, new ForegroundAppChangedEventArgs(executableName));
    }
}
