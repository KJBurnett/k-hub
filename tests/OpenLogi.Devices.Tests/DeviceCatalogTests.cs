using OpenLogi.Core.Capabilities;
using OpenLogi.Core.Devices;

namespace OpenLogi.Devices.Tests;

public class DeviceCatalogTests
{
    private readonly DeviceCatalog _catalog = new();

    [Fact]
    public void Known_device_resolves_to_named_tier1_device()
    {
        var info = _catalog.Resolve(
            new DeviceIdentity(0x046D, 0xC08B), DeviceConnection.UsbWired);

        Assert.Equal("G502 HERO", info.Name);
        Assert.Equal(DeviceTier.Tier1, info.Tier);
        Assert.True(info.Capabilities.Has(Capability.Dpi));
    }

    [Fact]
    public void Unknown_logitech_device_falls_back_to_generic_tier3()
    {
        var info = _catalog.Resolve(
            new DeviceIdentity(0x046D, 0x1234), DeviceConnection.UsbWired);

        Assert.Equal(DeviceTier.Tier3, info.Tier);
        Assert.True(info.Capabilities.Has(Capability.Dpi));
        Assert.Contains("1234", info.Name);
    }

    [Fact]
    public void Non_logitech_device_is_unknown_with_no_capabilities()
    {
        var info = _catalog.Resolve(
            new DeviceIdentity(0x1234, 0x5678), DeviceConnection.UsbWired);

        Assert.Equal(DeviceTier.Unknown, info.Tier);
        Assert.Empty(info.Capabilities.Capabilities);
    }

    [Fact]
    public void Wireless_connection_adds_battery_capability_for_unknown_device()
    {
        var info = _catalog.Resolve(
            new DeviceIdentity(0x046D, 0x1234), DeviceConnection.Lightspeed);

        Assert.True(info.Capabilities.Has(Capability.Battery));
    }
}
