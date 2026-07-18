using Microsoft.Extensions.Hosting;
using OpenLogi.Devices;
using OpenLogi.Logging;

namespace OpenLogi.Agent.Services;

/// <summary>
/// The long-running background agent (PLAN.md section 12). It starts device
/// tracking, watches the foreground application, and applies the appropriate
/// profile whenever devices connect or the foreground changes. It is designed
/// to sit idle at ~0% CPU (PLAN.md section 21) by reacting to events rather than
/// polling in a tight loop.
/// </summary>
public sealed class AgentWorker : BackgroundService
{
    private readonly DeviceManager _devices;
    private readonly ProfileSwitchingService _switcher;
    private readonly IForegroundAppMonitor _foreground;
    private readonly IAppLogger _logger;

    /// <summary>Creates the worker.</summary>
    public AgentWorker(
        DeviceManager devices,
        ProfileSwitchingService switcher,
        IForegroundAppMonitor foreground,
        IAppLoggerFactory loggerFactory)
    {
        _devices = devices;
        _switcher = switcher;
        _foreground = foreground;
        _logger = loggerFactory.CreateLogger<AgentWorker>();
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.Information("Background agent starting.");
        await _devices.StartAsync(stoppingToken).ConfigureAwait(false);

        _foreground.ForegroundChanged += OnForegroundChanged;
        _foreground.Start();

        // Apply the initial profile for whatever is currently in the foreground.
        await _switcher.ApplyForForegroundAsync(_foreground.CurrentExecutable, stoppingToken)
            .ConfigureAwait(false);

        try
        {
            // Idle until shutdown; all real work is event driven.
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown.
        }
        finally
        {
            _foreground.ForegroundChanged -= OnForegroundChanged;
            _foreground.Stop();
            _logger.Information("Background agent stopping.");
        }
    }

    private void OnForegroundChanged(object? sender, ForegroundAppChangedEventArgs e)
        => _ = ApplyForForegroundSafeAsync(e.ExecutableName);

    private async Task ApplyForForegroundSafeAsync(string? executableName)
    {
        try
        {
            await _switcher.ApplyForForegroundAsync(executableName, CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to switch profile on foreground change.", ex);
        }
    }
}
