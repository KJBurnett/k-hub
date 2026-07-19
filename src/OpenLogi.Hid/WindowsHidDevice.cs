using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace OpenLogi.Hid;

/// <summary>Windows file-handle implementation of an open HID device connection.</summary>
public sealed class WindowsHidDevice : IHidDevice
{
    private readonly FileStream _stream;

    /// <summary>Opens the HID device identified by <paramref name="descriptor"/>.</summary>
    public WindowsHidDevice(HidDeviceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        Descriptor = descriptor;
        _stream = new FileStream(
            descriptor.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read,
            bufferSize: 4096, FileOptions.Asynchronous);
    }

    /// <inheritdoc />
    public HidDeviceDescriptor Descriptor { get; }

    /// <inheritdoc />
    public bool IsOpen => _stream.SafeFileHandle is { IsClosed: false, IsInvalid: false };

    /// <inheritdoc />
    public async ValueTask<int> WriteAsync(
        ReadOnlyMemory<byte> report, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();
        await _stream.WriteAsync(report, cancellationToken).ConfigureAwait(false);
        return report.Length;
    }

    /// <inheritdoc />
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfClosed();
        return _stream.ReadAsync(buffer, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask SetFeatureAsync(
        ReadOnlyMemory<byte> report, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfClosed();
        var copy = report.ToArray();
        if (!HidD_SetFeature(_stream.SafeFileHandle, copy, copy.Length))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to set HID feature report.");
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<int> GetFeatureAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfClosed();
        var report = buffer.ToArray();
        if (!HidD_GetFeature(_stream.SafeFileHandle, report, report.Length))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to read HID feature report.");
        }

        report.CopyTo(buffer);
        return ValueTask.FromResult(buffer.Length);
    }

    /// <inheritdoc />
    public void Dispose() => _stream.Dispose();

    private void ThrowIfClosed()
    {
        if (!IsOpen)
        {
            throw new ObjectDisposedException(nameof(WindowsHidDevice));
        }
    }

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_SetFeature(SafeFileHandle handle, byte[] reportBuffer, int reportBufferLength);

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_GetFeature(SafeFileHandle handle, byte[] reportBuffer, int reportBufferLength);
}
