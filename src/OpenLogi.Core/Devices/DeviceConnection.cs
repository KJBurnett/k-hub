namespace OpenLogi.Core.Devices;

/// <summary>How a device is currently connected to the host (PLAN.md section 9).</summary>
public enum DeviceConnection
{
    /// <summary>Connection type could not be determined.</summary>
    Unknown = 0,

    /// <summary>Direct wired USB connection.</summary>
    UsbWired,

    /// <summary>Logitech LIGHTSPEED wireless connection.</summary>
    Lightspeed,

    /// <summary>Bluetooth connection.</summary>
    Bluetooth,

    /// <summary>Generic USB receiver / dongle.</summary>
    Receiver,
}
