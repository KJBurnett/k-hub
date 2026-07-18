using System.Collections.Concurrent;
using OpenLogi.Core.Events;
using OpenLogi.Hid;
using OpenLogi.Logging;

namespace OpenLogi.Devices;

/// <summary>
/// Tracks the set of connected devices. It listens to the HID backend for
/// arrivals and removals, creates and initialises devices through the factory,
/// and publishes <see cref="DeviceConnectedEvent"/> / <see cref="DeviceDisconnectedEvent"/>
/// so the agent and UI can react. It is designed to survive USB unplug,
/// receiver reconnect, sleep and wake (PLAN.md sections 9 and 22).
/// </summary>
public sealed class DeviceManager : IAsyncDisposable
{
    private readonly IHidBackend _backend;
    private readonly IDeviceFactory _factory;
    private readonly IEventBus _eventBus;
    private readonly IAppLogger _logger;
    private readonly ConcurrentDictionary<string, IDevice> _devices = new();
    private bool _started;

    /// <summary>Creates the device manager.</summary>
    public DeviceManager(
        IHidBackend backend,
        IDeviceFactory factory,
        IEventBus eventBus,
        IAppLoggerFactory loggerFactory)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _logger = loggerFactory.CreateLogger<DeviceManager>();
    }

    /// <summary>The devices currently connected and initialised.</summary>
    public IReadOnlyCollection<IDevice> ConnectedDevices => _devices.Values.ToArray();

    /// <summary>
    /// Begins tracking: subscribes to backend events and initialises any devices
    /// already present. Safe to call once.
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_started)
        {
            return;
        }

        _started = true;
        _backend.DeviceArrived += OnDeviceArrived;
        _backend.DeviceRemoved += OnDeviceRemoved;

        foreach (var descriptor in _backend.Enumerate())
        {
            await TryAddDeviceAsync(descriptor, cancellationToken).ConfigureAwait(false);
        }

        _logger.Information($"Device manager started with {_devices.Count} device(s).");
    }

    private void OnDeviceArrived(object? sender, HidDeviceEventArgs e)
        => _ = TryAddDeviceAsync(e.Descriptor, CancellationToken.None);

    private void OnDeviceRemoved(object? sender, HidDeviceEventArgs e)
        => _ = RemoveDeviceAsync(e.Descriptor.Path, e.Descriptor.Identity.StableKey);

    private async Task TryAddDeviceAsync(HidDeviceDescriptor descriptor, CancellationToken cancellationToken)
    {
        try
        {
            var device = _factory.Create(descriptor);
            if (device is null)
            {
                return;
            }

            await device.InitializeAsync(cancellationToken).ConfigureAwait(false);
            _devices[descriptor.Path] = device;
            _eventBus.Publish(new DeviceConnectedEvent(device.Info));
            _logger.Information($"Device connected: {device.Info.Name} [{device.Info.Identity.StableKey}]");
        }
        catch (Exception ex)
        {
            // Treat every hardware interaction as fallible (Appendix A #5).
            _logger.Error($"Failed to initialise device at '{descriptor.Path}'.", ex);
        }
    }

    private async Task RemoveDeviceAsync(string path, string stableKey)
    {
        if (!_devices.TryRemove(path, out var device))
        {
            return;
        }

        var identity = device.Info.Identity;
        try
        {
            await device.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.Warning($"Error disposing device '{stableKey}'.", ex);
        }

        _eventBus.Publish(new DeviceDisconnectedEvent(identity));
        _logger.Information($"Device disconnected: {identity.StableKey}");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_started)
        {
            _backend.DeviceArrived -= OnDeviceArrived;
            _backend.DeviceRemoved -= OnDeviceRemoved;
        }

        foreach (var device in _devices.Values)
        {
            try
            {
                await device.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Warning("Error disposing device during shutdown.", ex);
            }
        }

        _devices.Clear();
    }
}
