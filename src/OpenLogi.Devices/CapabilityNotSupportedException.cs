using OpenLogi.Core.Capabilities;

namespace OpenLogi.Devices;

/// <summary>
/// Thrown when a caller asks a device to perform an operation whose capability
/// the device does not advertise. Callers should normally gate on the
/// capability graph and never see this; it exists to fail loudly when a write
/// operation is requested on unsupported hardware.
/// </summary>
public sealed class CapabilityNotSupportedException : InvalidOperationException
{
    /// <summary>Creates the exception for a missing capability.</summary>
    public CapabilityNotSupportedException(Capability capability)
        : base($"The device does not support the '{capability}' capability.")
    {
        Capability = capability;
    }

    /// <summary>The capability that was required but not present.</summary>
    public Capability Capability { get; }
}
