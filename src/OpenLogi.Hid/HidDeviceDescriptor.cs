using OpenLogi.Core.Devices;

namespace OpenLogi.Hid;

/// <summary>
/// A raw description of a HID device as reported by the operating system. The
/// HID layer performs raw communication only and holds no business logic
/// (PLAN.md section 11); interpreting these fields is the device layer's job.
/// </summary>
public sealed record HidDeviceDescriptor
{
    /// <summary>Hardware identity of the device.</summary>
    public required DeviceIdentity Identity { get; init; }

    /// <summary>OS-specific device path used to open the device.</summary>
    public required string Path { get; init; }

    /// <summary>HID usage page from the report descriptor.</summary>
    public ushort UsagePage { get; init; }

    /// <summary>HID usage from the report descriptor.</summary>
    public ushort Usage { get; init; }

    /// <summary>Manufacturer string, when the device exposes one.</summary>
    public string? Manufacturer { get; init; }

    /// <summary>Product string, when the device exposes one.</summary>
    public string? Product { get; init; }

    /// <summary>Length in bytes of an input report (0 when unknown).</summary>
    public int InputReportLength { get; init; }

    /// <summary>Length in bytes of an output report (0 when unknown).</summary>
    public int OutputReportLength { get; init; }

    /// <summary>Length in bytes of a feature report (0 when unknown).</summary>
    public int FeatureReportLength { get; init; }
}
