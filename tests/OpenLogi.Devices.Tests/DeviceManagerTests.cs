using OpenLogi.Core.Devices;
using OpenLogi.Core.Events;
using OpenLogi.Hid;
using OpenLogi.Hid.Testing;
using OpenLogi.Logging;

namespace OpenLogi.Devices.Tests;

public class DeviceManagerTests
{
    private static readonly IAppLoggerFactory LoggerFactory = new AppLoggerFactory(LogLevel.Warning);

    private static HidDeviceDescriptor Descriptor(ushort vendorId, ushort productId, string path) => new()
    {
        Identity = new DeviceIdentity(vendorId, productId),
        Path = path,
    };

    private static DeviceManager CreateManager(MockHidBackend backend, IEventBus bus)
    {
        var factory = new DeviceFactory(backend, new DeviceCatalog(), LoggerFactory);
        return new DeviceManager(backend, factory, bus, LoggerFactory);
    }

    [Fact]
    public async Task StartAsync_initialises_preconnected_logitech_device()
    {
        var backend = new MockHidBackend();
        var bus = new EventBus();
        var connected = 0;
        bus.Subscribe<DeviceConnectedEvent>(_ => connected++);
        backend.Connect(Descriptor(0x046D, 0xC08B, "mock://1"));

        await using var manager = CreateManager(backend, bus);
        await manager.StartAsync();

        Assert.Single(manager.ConnectedDevices);
        Assert.Equal(1, connected);
    }

    [Fact]
    public async Task Non_logitech_device_is_ignored()
    {
        var backend = new MockHidBackend();
        var bus = new EventBus();
        backend.Connect(Descriptor(0x1234, 0x5678, "mock://1"));

        await using var manager = CreateManager(backend, bus);
        await manager.StartAsync();

        Assert.Empty(manager.ConnectedDevices);
    }

    [Fact]
    public async Task Disconnect_removes_device_and_publishes_event()
    {
        var backend = new MockHidBackend();
        var bus = new EventBus();
        var disconnected = 0;
        bus.Subscribe<DeviceDisconnectedEvent>(_ => disconnected++);
        var descriptor = Descriptor(0x046D, 0xC08B, "mock://1");
        backend.Connect(descriptor);

        await using var manager = CreateManager(backend, bus);
        await manager.StartAsync();
        backend.Disconnect(descriptor);

        Assert.Empty(manager.ConnectedDevices);
        Assert.Equal(1, disconnected);
    }
}
