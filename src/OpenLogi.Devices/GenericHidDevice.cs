using OpenLogi.Core.Capabilities;
using OpenLogi.Core.Configuration;
using OpenLogi.Core.Devices;
using OpenLogi.Hid;
using OpenLogi.Logging;

namespace OpenLogi.Devices;

/// <summary>
/// A generic, capability-driven <see cref="IDevice"/> that works for any mouse
/// over an <see cref="IHidDevice"/> transport. It contains no model-specific
/// branching: every operation is gated on the capability graph, so the same
/// implementation serves Tier 1, 2 and 3 devices and degrades gracefully for
/// unknown hardware (PLAN.md sections 8, 13, 14).
/// </summary>
public sealed class GenericHidDevice : IDevice
{
    private readonly IHidDevice _transport;
    private readonly IAppLogger _logger;
    private bool _initialized;

    /// <summary>Creates a generic device over an open HID transport.</summary>
    public GenericHidDevice(DeviceInfo info, IHidDevice transport, IAppLogger logger)
    {
        Info = info ?? throw new ArgumentNullException(nameof(info));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public DeviceInfo Info { get; }

    /// <inheritdoc />
    public CapabilityGraph Capabilities => Info.Capabilities;

    /// <inheritdoc />
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _initialized = true;
        _logger.Information($"Device initialized: {Info.Name} [{Info.Identity.StableKey}]");
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<CapabilityGraph> GetCapabilitiesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Capabilities);

    /// <inheritdoc />
    public async Task ApplySettingsAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        EnsureInitialized();

        if (Capabilities.Has(Capability.Dpi) && profile.Dpi.ActiveStage is { } stage)
        {
            await _transport.WriteAsync(DeviceReports.SetDpi(stage.Dpi), cancellationToken)
                .ConfigureAwait(false);
            _logger.Information($"Applied DPI {stage.Dpi} ('{stage.Name}') to {Info.Name}");
        }

        if (Capabilities.Has(Capability.PollingRate))
        {
            await _transport.WriteAsync(
                DeviceReports.SetPollingRate(profile.PollingRate.Hz), cancellationToken)
                .ConfigureAwait(false);
            _logger.Information($"Applied polling rate {profile.PollingRate} to {Info.Name}");
        }

        if (Capabilities.Has(Capability.ButtonRemapping) && profile.ButtonMappings.Count > 0)
        {
            await WriteButtonMappingsAsync(profile.ButtonMappings, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task<BatteryStatus> ReadBatteryAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        if (!Capabilities.Has(Capability.Battery))
        {
            return BatteryStatus.Unknown;
        }

        var buffer = new byte[8];
        var read = await _transport.GetFeatureAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (read < 3)
        {
            return BatteryStatus.Unknown;
        }

        // Provisional decode: buffer[1] = percentage, buffer[2] = charging flag.
        var percent = Math.Clamp(buffer[1], (byte)0, (byte)100);
        return new BatteryStatus(percent, buffer[2] != 0);
    }

    /// <inheritdoc />
    public async Task<int?> ReadCurrentDpiAsync(CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        if (!Capabilities.Has(Capability.Dpi))
        {
            return null;
        }

        var buffer = new byte[8];
        var read = await _transport.GetFeatureAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (read < 3)
        {
            return null;
        }

        // Provisional decode: little-endian DPI in buffer[1..2].
        return buffer[1] | (buffer[2] << 8);
    }

    /// <inheritdoc />
    public async Task SetPollingRateAsync(PollingRate rate, CancellationToken cancellationToken = default)
    {
        EnsureInitialized();
        RequireCapability(Capability.PollingRate);
        await _transport.WriteAsync(DeviceReports.SetPollingRate(rate.Hz), cancellationToken)
            .ConfigureAwait(false);
        _logger.Information($"Set polling rate {rate} on {Info.Name}");
    }

    /// <inheritdoc />
    public async Task MapButtonsAsync(
        IEnumerable<ButtonMapping> mappings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mappings);
        EnsureInitialized();
        RequireCapability(Capability.ButtonRemapping);
        await WriteButtonMappingsAsync(mappings, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _transport.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task WriteButtonMappingsAsync(
        IEnumerable<ButtonMapping> mappings, CancellationToken cancellationToken)
    {
        foreach (var mapping in mappings)
        {
            await _transport.WriteAsync(
                DeviceReports.MapButton(mapping.ButtonId, (byte)mapping.Action.Kind),
                cancellationToken).ConfigureAwait(false);
        }

        _logger.Information($"Applied button mappings to {Info.Name}");
    }

    private void EnsureInitialized()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException(
                "The device must be initialized before use. Call InitializeAsync first.");
        }
    }

    private void RequireCapability(Capability capability)
    {
        if (!Capabilities.Has(capability))
        {
            throw new CapabilityNotSupportedException(capability);
        }
    }
}
