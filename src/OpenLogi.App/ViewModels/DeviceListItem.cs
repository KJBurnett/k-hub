using OpenLogi.Core.Devices;

namespace OpenLogi.App.ViewModels;

/// <summary>
/// A read-only, display-friendly projection of a connected device for the
/// device list. Keeping this separate from <see cref="DeviceInfo"/> means the
/// UI never depends on device-layer internals.
/// </summary>
public sealed class DeviceListItem
{
    /// <summary>Creates a list item from device info.</summary>
    public DeviceListItem(DeviceInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        Name = info.Name;
        Tier = info.Tier.ToString();
        Connection = info.Connection.ToString();
        StableKey = info.Identity.StableKey;
        Capabilities = string.Join(", ", info.Capabilities.Capabilities.Select(c => c.ToString()));
    }

    /// <summary>Friendly product name.</summary>
    public string Name { get; }

    /// <summary>Support tier as text.</summary>
    public string Tier { get; }

    /// <summary>Connection type as text.</summary>
    public string Connection { get; }

    /// <summary>Stable device key.</summary>
    public string StableKey { get; }

    /// <summary>Comma-separated list of advertised capabilities.</summary>
    public string Capabilities { get; }
}
