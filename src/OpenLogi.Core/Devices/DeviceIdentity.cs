namespace OpenLogi.Core.Devices;

/// <summary>
/// Stable hardware identity of a device. USB vendor id for Logitech is 0x046D.
/// The <see cref="SerialNumber"/> (when available) lets OpenLogi tell two
/// identical models apart and remember per-device configuration.
/// </summary>
public sealed record DeviceIdentity(
    ushort VendorId,
    ushort ProductId,
    string? SerialNumber = null)
{
    /// <summary>USB vendor id used by Logitech peripherals.</summary>
    public const ushort LogitechVendorId = 0x046D;

    /// <summary>True when this identity belongs to a Logitech device.</summary>
    public bool IsLogitech => VendorId == LogitechVendorId;

    /// <summary>
    /// A stable key for this device, preferring the serial number and falling
    /// back to the vendor/product pair when no serial is exposed.
    /// </summary>
    public string StableKey => string.IsNullOrWhiteSpace(SerialNumber)
        ? $"{VendorId:X4}:{ProductId:X4}"
        : $"{VendorId:X4}:{ProductId:X4}:{SerialNumber}";
}
