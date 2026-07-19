using OpenLogi.Core.Devices;
using OpenLogi.Hid;
using OpenLogi.Hid.Testing;

namespace OpenLogi.Hid.Tests;

public class MockHidBackendTests
{
    private static HidDeviceDescriptor Descriptor(string path) => new()
    {
        Identity = new DeviceIdentity(0x046D, 0xC08B),
        Path = path,
    };

    [Fact]
    public void Connect_adds_to_enumeration_and_raises_event()
    {
        var backend = new MockHidBackend();
        HidDeviceDescriptor? arrived = null;
        backend.DeviceArrived += (_, e) => arrived = e.Descriptor;

        backend.Connect(Descriptor("mock://1"));

        Assert.Single(backend.Enumerate());
        Assert.Equal("mock://1", arrived!.Path);
    }

    [Fact]
    public void Disconnect_removes_from_enumeration_and_raises_event()
    {
        var backend = new MockHidBackend();
        var descriptor = Descriptor("mock://1");
        backend.Connect(descriptor);
        var removed = false;
        backend.DeviceRemoved += (_, _) => removed = true;

        backend.Disconnect(descriptor);

        Assert.Empty(backend.Enumerate());
        Assert.True(removed);
    }

    [Fact]
    public void Open_returns_tracked_device()
    {
        var backend = new MockHidBackend();
        var descriptor = Descriptor("mock://1");
        backend.Connect(descriptor);

        using var device = backend.Open(descriptor);

        Assert.True(device.IsOpen);
        Assert.Same(device, backend.GetOpenDevice("mock://1"));
    }
}
