using OpenLogi.Core.Devices;
using OpenLogi.Hid;
using OpenLogi.Logging;

namespace OpenLogi.Devices;

/// <summary>Creates an <see cref="IDevice"/> for a discovered HID descriptor.</summary>
public interface IDeviceFactory
{
    /// <summary>
    /// Returns a device for <paramref name="descriptor"/>, or null when the
    /// descriptor is not a device OpenLogi manages (e.g. a non-Logitech device).
    /// </summary>
    IDevice? Create(HidDeviceDescriptor descriptor);
}

/// <summary>
/// Default factory. Resolves generic device info from the <see cref="DeviceCatalog"/>,
/// opens the HID transport through the backend, and wraps both in a
/// <see cref="GenericHidDevice"/>. Only Logitech devices are handled; everything
/// else returns null (PLAN.md section 4 — mice only, Logitech only).
/// </summary>
public sealed class DeviceFactory : IDeviceFactory
{
    private readonly IHidBackend _backend;
    private readonly DeviceCatalog _catalog;
    private readonly IAppLoggerFactory _loggerFactory;

    /// <summary>Creates the factory.</summary>
    public DeviceFactory(IHidBackend backend, DeviceCatalog catalog, IAppLoggerFactory loggerFactory)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <inheritdoc />
    public IDevice? Create(HidDeviceDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (!descriptor.Identity.IsLogitech)
        {
            return null;
        }

        var connection = InferConnection(descriptor);
        var info = _catalog.Resolve(descriptor.Identity, connection);
        var transport = _backend.Open(descriptor);
        var logger = _loggerFactory.CreateLogger<GenericHidDevice>();
        return new GenericHidDevice(info, transport, logger);
    }

    private static DeviceConnection InferConnection(HidDeviceDescriptor descriptor)
    {
        var product = descriptor.Product ?? string.Empty;
        if (product.Contains("LIGHTSPEED", StringComparison.OrdinalIgnoreCase))
        {
            return DeviceConnection.Lightspeed;
        }

        if (product.Contains("Receiver", StringComparison.OrdinalIgnoreCase))
        {
            return DeviceConnection.Receiver;
        }

        if (product.Contains("Bluetooth", StringComparison.OrdinalIgnoreCase))
        {
            return DeviceConnection.Bluetooth;
        }

        return DeviceConnection.UsbWired;
    }
}
