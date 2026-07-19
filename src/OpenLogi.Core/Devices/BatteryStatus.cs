namespace OpenLogi.Core.Devices;

/// <summary>
/// Battery telemetry for a wireless device. Only meaningful when the device
/// advertises <see cref="Capabilities.Capability.Battery"/>; every field is
/// optional so unknown devices can report whatever they expose.
/// </summary>
public sealed record BatteryStatus(
    int? PercentRemaining = null,
    bool IsCharging = false,
    int? MillivoltsRemaining = null)
{
    /// <summary>Represents a device whose battery state is currently unknown.</summary>
    public static readonly BatteryStatus Unknown = new();

    /// <summary>True when a usable percentage reading is available.</summary>
    public bool HasReading => PercentRemaining is >= 0 and <= 100;
}
