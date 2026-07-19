using OpenLogi.Core.Capabilities;

namespace OpenLogi.Core.Devices;

/// <summary>
/// An immutable description of a connected device as understood by the layers
/// above the device layer. It carries the device identity, a friendly name,
/// its support tier, how it is connected, and the capability graph discovered
/// at connection time. No device-specific behaviour leaks upward (PLAN.md
/// sections 11-13); consumers only see this generic view.
/// </summary>
public sealed record DeviceInfo
{
    /// <summary>Hardware identity of the device.</summary>
    public required DeviceIdentity Identity { get; init; }

    /// <summary>Human-friendly product name (e.g. "G502 HERO").</summary>
    public required string Name { get; init; }

    /// <summary>Support tier, advisory metadata only.</summary>
    public DeviceTier Tier { get; init; } = DeviceTier.Unknown;

    /// <summary>Current connection type.</summary>
    public DeviceConnection Connection { get; init; } = DeviceConnection.Unknown;

    /// <summary>Firmware version string, when the device reports one.</summary>
    public string? FirmwareVersion { get; init; }

    /// <summary>Capabilities discovered for this device at connection time.</summary>
    public CapabilityGraph Capabilities { get; init; } = CapabilityGraph.Empty;
}
