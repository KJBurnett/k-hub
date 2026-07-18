using OpenLogi.Core.Capabilities;
using OpenLogi.Core.Configuration;
using OpenLogi.Core.Devices;
using OpenLogi.Hid;
using OpenLogi.Hid.Testing;
using OpenLogi.Logging;

namespace OpenLogi.Devices.Tests;

public class GenericHidDeviceTests
{
    private static readonly IAppLoggerFactory LoggerFactory = new AppLoggerFactory(LogLevel.Warning);

    private static (GenericHidDevice Device, MockHidDevice Transport) Create(params Capability[] capabilities)
    {
        var descriptor = new HidDeviceDescriptor
        {
            Identity = new DeviceIdentity(0x046D, 0xC08B),
            Path = "mock://1",
        };
        var transport = new MockHidDevice(descriptor);
        var info = new DeviceInfo
        {
            Identity = descriptor.Identity,
            Name = "Test Mouse",
            Tier = DeviceTier.Tier1,
            Connection = DeviceConnection.UsbWired,
            Capabilities = CapabilityGraph.FromCapabilities(capabilities),
        };
        var logger = LoggerFactory.CreateLogger<GenericHidDevice>();
        return (new GenericHidDevice(info, transport, logger), transport);
    }

    [Fact]
    public async Task Operations_require_initialization()
    {
        var (device, _) = Create(Capability.PollingRate);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await device.SetPollingRateAsync(new PollingRate(1000)));
    }

    [Fact]
    public async Task ApplySettings_writes_dpi_report_to_transport()
    {
        var (device, transport) = Create(Capability.Dpi);
        await device.InitializeAsync();
        var profile = new Profile("p1", "Test")
        {
            Dpi = new DpiSettings(new[] { new DpiStage("Main", 1600) }, 0),
        };

        await device.ApplySettingsAsync(profile);

        var report = Assert.Single(transport.WrittenReports);
        Assert.Equal(0x11, report[0]);
        Assert.Equal(0x01, report[1]);
        Assert.Equal(1600 & 0xFF, report[2]);
        Assert.Equal((1600 >> 8) & 0xFF, report[3]);
    }

    [Fact]
    public async Task SetPollingRate_throws_when_capability_missing()
    {
        var (device, _) = Create(Capability.Dpi);
        await device.InitializeAsync();

        await Assert.ThrowsAsync<CapabilityNotSupportedException>(
            async () => await device.SetPollingRateAsync(new PollingRate(1000)));
    }

    [Fact]
    public async Task ReadBattery_returns_unknown_when_unsupported()
    {
        var (device, _) = Create(Capability.Dpi);
        await device.InitializeAsync();

        var battery = await device.ReadBatteryAsync();

        Assert.False(battery.HasReading);
    }

    [Fact]
    public async Task ReadBattery_decodes_feature_report_when_supported()
    {
        var (device, transport) = Create(Capability.Battery);
        await device.InitializeAsync();
        await transport.SetFeatureAsync(new byte[] { 0x00, 77, 1 });

        var battery = await device.ReadBatteryAsync();

        Assert.Equal(77, battery.PercentRemaining);
        Assert.True(battery.IsCharging);
    }
}
