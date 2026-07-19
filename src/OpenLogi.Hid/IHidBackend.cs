namespace OpenLogi.Hid;

/// <summary>Event data describing a HID device arriving or being removed.</summary>
public sealed class HidDeviceEventArgs : EventArgs
{
    /// <summary>Creates the event args.</summary>
    public HidDeviceEventArgs(HidDeviceDescriptor descriptor) => Descriptor = descriptor;

    /// <summary>The affected device descriptor.</summary>
    public HidDeviceDescriptor Descriptor { get; }
}

/// <summary>
/// Enumerates HID devices and opens connections to them. This is the single
/// seam between OpenLogi and the operating system's HID stack; the real Windows
/// implementation (Phase 2) and the in-memory mock used by tests both sit
/// behind this interface so nothing above needs a physical device.
/// </summary>
public interface IHidBackend
{
    /// <summary>Returns the HID devices currently present on the system.</summary>
    IReadOnlyList<HidDeviceDescriptor> Enumerate();

    /// <summary>Opens a connection to the device described by <paramref name="descriptor"/>.</summary>
    IHidDevice Open(HidDeviceDescriptor descriptor);

    /// <summary>Raised when a device is connected.</summary>
    event EventHandler<HidDeviceEventArgs>? DeviceArrived;

    /// <summary>Raised when a device is removed.</summary>
    event EventHandler<HidDeviceEventArgs>? DeviceRemoved;
}
