using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenLogi.Agent.Services;
using OpenLogi.Core.Events;
using OpenLogi.Devices;
using OpenLogi.Hid;
using OpenLogi.Hid.Testing;
using OpenLogi.Logging;
using OpenLogi.Logging.Sinks;
using OpenLogi.Storage;
using OpenLogi.Storage.Repositories;

namespace OpenLogi.Agent;

/// <summary>
/// The single composition root for OpenLogi. Both the standalone agent host and
/// the desktop UI register the same services here so the whole stack (Core →
/// Agent → Devices → HID, plus Logging and Storage) is wired identically and in
/// one place. The HID backend is currently the in-memory mock; the real Windows
/// backend will be swapped in during Phase 2 without touching callers.
/// </summary>
public static class OpenLogiServices
{
    /// <summary>Registers every OpenLogi service into <paramref name="services"/>.</summary>
    public static IServiceCollection AddOpenLogi(
        this IServiceCollection services, Action<OpenLogiOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new OpenLogiOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Logging: shared in-memory sink backs diagnostics export (PLAN.md section 17).
        var inMemorySink = new InMemoryLogSink();
        services.AddSingleton(inMemorySink);
        services.AddSingleton(_ => new FileLogSink(options.LogFilePath));
        services.AddSingleton<IAppLoggerFactory>(_ => new AppLoggerFactory(
            options.MinimumLogLevel,
            new ConsoleLogSink(),
            _.GetRequiredService<FileLogSink>(),
            inMemorySink));

        // Core.
        services.AddSingleton<IEventBus, EventBus>();

        // Storage.
        services.AddSingleton(sp => OpenLogiDatabase.ForFile(
            options.DatabasePath, sp.GetRequiredService<IAppLoggerFactory>()));
        services.AddSingleton<IProfileRepository, ProfileRepository>();
        services.AddSingleton<ISettingsRepository, SettingsRepository>();
        services.AddSingleton<IApplicationRuleRepository, ApplicationRuleRepository>();

        // Use the real Windows transport for normal production runs. Demo mode
        // and non-Windows development continue to use the hardware-free backend.
        if (OperatingSystem.IsWindows() && !options.UseDemoDevices)
        {
            services.AddSingleton<IHidBackend, WindowsHidBackend>();
        }
        else
        {
            services.AddSingleton<IHidBackend, MockHidBackend>();
        }

        // Device layer.
        services.AddSingleton<DeviceCatalog>();
        services.AddSingleton<IDeviceFactory, DeviceFactory>();
        services.AddSingleton<DeviceManager>();

        // Agent services.
        services.AddSingleton<IForegroundAppMonitor, NullForegroundAppMonitor>();
        services.AddSingleton<ProfileSwitchingService>();

        // Hosted services run in registration order: initialise the store first.
        services.AddHostedService<StartupInitializer>();
        services.AddHostedService<AgentWorker>();

        return services;
    }
}
