using System.Collections.Concurrent;

namespace OpenLogi.Hid.Testing;

/// <summary>
/// An in-memory <see cref="IHidBackend"/> for automated tests and a hardware-free
/// demo mode. Descriptors can be added and removed at runtime to simulate the
/// connect / disconnect / reconnect scenarios OpenLogi must survive
/// (PLAN.md sections 19 and 22).
/// </summary>
public sealed class MockHidBackend : IHidBackend
{
    private readonly List<HidDeviceDescriptor> _descriptors = new();
    private readonly ConcurrentDictionary<string, MockHidDevice> _openDevices = new();
    private readonly object _gate = new();

    /// <inheritdoc />
    public event EventHandler<HidDeviceEventArgs>? DeviceArrived;

    /// <inheritdoc />
    public event EventHandler<HidDeviceEventArgs>? DeviceRemoved;

    /// <summary>Simulates a device being connected, raising <see cref="DeviceArrived"/>.</summary>
    public void Connect(HidDeviceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        lock (_gate)
        {
            _descriptors.Add(descriptor);
        }

        DeviceArrived?.Invoke(this, new HidDeviceEventArgs(descriptor));
    }

    /// <summary>Simulates a device being removed, raising <see cref="DeviceRemoved"/>.</summary>
    public void Disconnect(HidDeviceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        bool removed;
        lock (_gate)
        {
            removed = _descriptors.RemoveAll(d => d.Path == descriptor.Path) > 0;
        }

        if (_openDevices.TryRemove(descriptor.Path, out var device))
        {
            device.Dispose();
        }

        if (removed)
        {
            DeviceRemoved?.Invoke(this, new HidDeviceEventArgs(descriptor));
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<HidDeviceDescriptor> Enumerate()
    {
        lock (_gate)
        {
            return _descriptors.ToArray();
        }
    }

    /// <inheritdoc />
    public IHidDevice Open(HidDeviceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        var device = new MockHidDevice(descriptor);
        _openDevices[descriptor.Path] = device;
        return device;
    }

    /// <summary>Returns the currently open mock device for a path, if any.</summary>
    public MockHidDevice? GetOpenDevice(string path)
        => _openDevices.TryGetValue(path, out var device) ? device : null;
}
