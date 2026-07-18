using Microsoft.Extensions.Hosting;
using OpenLogi.Core.Configuration;
using OpenLogi.Core.Devices;
using OpenLogi.Hid;
using OpenLogi.Hid.Testing;
using OpenLogi.Logging;
using OpenLogi.Storage;
using OpenLogi.Storage.Repositories;

namespace OpenLogi.Agent.Services;

/// <summary>
/// Runs once at startup: initialises the local database, guarantees a default
/// profile exists, and (in demo mode) seeds a mock device and application rule.
/// Registered before <see cref="AgentWorker"/> so the store is ready first.
/// </summary>
public sealed class StartupInitializer : IHostedService
{
    private readonly OpenLogiDatabase _database;
    private readonly IProfileRepository _profiles;
    private readonly IApplicationRuleRepository _rules;
    private readonly IHidBackend _backend;
    private readonly OpenLogiOptions _options;
    private readonly IAppLogger _logger;

    /// <summary>Creates the initializer.</summary>
    public StartupInitializer(
        OpenLogiDatabase database,
        IProfileRepository profiles,
        IApplicationRuleRepository rules,
        IHidBackend backend,
        OpenLogiOptions options,
        IAppLoggerFactory loggerFactory)
    {
        _database = database;
        _profiles = profiles;
        _rules = rules;
        _backend = backend;
        _options = options;
        _logger = loggerFactory.CreateLogger<StartupInitializer>();
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _database.InitializeAsync(cancellationToken).ConfigureAwait(false);
        await EnsureDefaultProfileAsync(cancellationToken).ConfigureAwait(false);

        if (_options.UseDemoDevices)
        {
            await SeedDemoAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task EnsureDefaultProfileAsync(CancellationToken cancellationToken)
    {
        var existing = await _profiles.GetDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return;
        }

        var all = await _profiles.GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (all.Count > 0)
        {
            await _profiles.SetDefaultAsync(all[0].Id, cancellationToken).ConfigureAwait(false);
            return;
        }

        var defaultProfile = new Profile(Guid.NewGuid().ToString("N"), "Default")
        {
            IsDefault = true,
            Dpi = DpiSettings.Default,
            PollingRate = new PollingRate(1000),
        };
        await _profiles.SaveAsync(defaultProfile, cancellationToken).ConfigureAwait(false);
        _logger.Information("Created initial default profile.");
    }

    private async Task SeedDemoAsync(CancellationToken cancellationToken)
    {
        if (_backend is not MockHidBackend mock)
        {
            return;
        }

        var descriptor = new HidDeviceDescriptor
        {
            Identity = new DeviceIdentity(DeviceIdentity.LogitechVendorId, 0xC08B, "DEMO-0001"),
            Path = "demo://g502-hero",
            Product = "G502 HERO",
            Manufacturer = "Logitech",
            InputReportLength = 20,
            OutputReportLength = 20,
            FeatureReportLength = 20,
        };
        mock.Connect(descriptor);

        var gaming = new Profile(Guid.NewGuid().ToString("N"), "Gaming")
        {
            Dpi = new DpiSettings(
                new[] { new DpiStage("Precision", 800), new DpiStage("Sniper", 400), new DpiStage("Fast", 3200) },
                0),
            PollingRate = new PollingRate(1000),
        };
        await _profiles.SaveAsync(gaming, cancellationToken).ConfigureAwait(false);
        await _rules.UpsertAsync(new ApplicationProfileRule("game.exe", gaming.Id), cancellationToken)
            .ConfigureAwait(false);

        _logger.Information("Seeded demo device and sample profile.");
    }
}
