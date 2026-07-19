using OpenLogi.Core.Devices;
using OpenLogi.Hid;
using OpenLogi.Hid.Testing;

namespace OpenLogi.Hid.Tests;

public class MockHidDeviceTests
{
    private static HidDeviceDescriptor Descriptor() => new()
    {
        Identity = new DeviceIdentity(0x046D, 0xC08B, "SN1"),
        Path = "mock://1",
    };

    [Fact]
    public async Task WriteAsync_records_reports()
    {
        using var device = new MockHidDevice(Descriptor());

        await device.WriteAsync(new byte[] { 1, 2, 3 });

        var written = Assert.Single(device.WrittenReports);
        Assert.Equal(new byte[] { 1, 2, 3 }, written);
    }

    [Fact]
    public async Task ReadAsync_returns_queued_reports_then_zero()
    {
        using var device = new MockHidDevice(Descriptor());
        device.EnqueueInputReport(9, 8, 7);

        var buffer = new byte[8];
        var first = await device.ReadAsync(buffer);
        var second = await device.ReadAsync(buffer);

        Assert.Equal(3, first);
        Assert.Equal(0, second);
        Assert.Equal(9, buffer[0]);
    }

    [Fact]
    public async Task Feature_report_round_trips()
    {
        using var device = new MockHidDevice(Descriptor());
        await device.SetFeatureAsync(new byte[] { 5, 6 });

        var buffer = new byte[8];
        var read = await device.GetFeatureAsync(buffer);

        Assert.Equal(2, read);
        Assert.Equal(5, buffer[0]);
    }

    [Fact]
    public async Task Using_after_dispose_throws()
    {
        var device = new MockHidDevice(Descriptor());
        device.Dispose();

        Assert.False(device.IsOpen);
        await Assert.ThrowsAsync<ObjectDisposedException>(
            async () => await device.WriteAsync(new byte[] { 1 }));
    }
}
