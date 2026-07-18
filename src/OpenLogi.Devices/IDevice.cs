using OpenLogi.Core.Capabilities;
using OpenLogi.Core.Configuration;
using OpenLogi.Core.Devices;

namespace OpenLogi.Devices;

/// <summary>
/// The abstraction every mouse implements (PLAN.md section 13). Everything above
/// the device layer works purely against this interface, so no device-specific
/// code leaks upward (section 11). Operations are asynchronous and cancellable
/// because every hardware interaction is fallible (Appendix A #5).
/// </summary>
public interface IDevice : IAsyncDisposable
{
    /// <summary>Generic information about the device, including its capability graph.</summary>
    DeviceInfo Info { get; }

    /// <summary>Capabilities discovered for this device.</summary>
    CapabilityGraph Capabilities { get; }

    /// <summary>Opens the underlying transport and discovers capabilities.</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Re-reads and returns the device capability graph.</summary>
    Task<CapabilityGraph> GetCapabilitiesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies every part of <paramref name="profile"/> the device supports,
    /// silently skipping unsupported parts so the call degrades gracefully.
    /// </summary>
    Task ApplySettingsAsync(Profile profile, CancellationToken cancellationToken = default);

    /// <summary>Reads battery status, or <see cref="BatteryStatus.Unknown"/> when unsupported.</summary>
    Task<BatteryStatus> ReadBatteryAsync(CancellationToken cancellationToken = default);

    /// <summary>Reads the current DPI, or null when the device cannot report it.</summary>
    Task<int?> ReadCurrentDpiAsync(CancellationToken cancellationToken = default);

    /// <summary>Sets the report (polling) rate.</summary>
    /// <exception cref="CapabilityNotSupportedException">When polling rate is not supported.</exception>
    Task SetPollingRateAsync(PollingRate rate, CancellationToken cancellationToken = default);

    /// <summary>Applies button mappings.</summary>
    /// <exception cref="CapabilityNotSupportedException">When button remapping is not supported.</exception>
    Task MapButtonsAsync(IEnumerable<ButtonMapping> mappings, CancellationToken cancellationToken = default);
}
