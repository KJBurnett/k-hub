namespace OpenLogi.Hid;

/// <summary>
/// An open connection to a single HID device. All hardware interaction is
/// treated as fallible (Appendix A #5): callers should expect exceptions and
/// cancellation. Implementations must be disposed to release the OS handle.
/// </summary>
public interface IHidDevice : IDisposable
{
    /// <summary>The descriptor this connection was opened from.</summary>
    HidDeviceDescriptor Descriptor { get; }

    /// <summary>True while the underlying OS handle is open.</summary>
    bool IsOpen { get; }

    /// <summary>Writes a single output report to the device.</summary>
    /// <returns>The number of bytes written.</returns>
    ValueTask<int> WriteAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken = default);

    /// <summary>Reads a single input report from the device into <paramref name="buffer"/>.</summary>
    /// <returns>The number of bytes read; 0 indicates no report was available.</returns>
    ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);

    /// <summary>Sends a feature report to the device.</summary>
    ValueTask SetFeatureAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken = default);

    /// <summary>Reads a feature report from the device into <paramref name="buffer"/>.</summary>
    /// <returns>The number of bytes read.</returns>
    ValueTask<int> GetFeatureAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
}
