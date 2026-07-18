using OpenLogi.Core.Capabilities;
using OpenLogi.Core.Devices;

namespace OpenLogi.Devices;

/// <summary>
/// A small, static description of a device OpenLogi recognises. This is only a
/// hint used to assign a friendly name, a support tier, and an initial
/// capability set. Actual behaviour is always driven by the capability graph
/// (PLAN.md sections 8 and 14), never by matching against this table, so
/// unrecognised devices still work.
/// </summary>
public sealed record KnownDevice(
    ushort ProductId,
    string Name,
    DeviceTier Tier,
    IReadOnlyList<Capability> Capabilities);

/// <summary>
/// Maps a HID device identity to a friendly name, support tier and starting
/// capability graph. Seeded with a representative sample of PLAN.md's Tier 1/2
/// devices; any other Logitech mouse falls back to a generic Tier 3 profile so
/// the software degrades gracefully (PLAN.md sections 7 and 8).
/// </summary>
public sealed class DeviceCatalog
{
    // Baseline capabilities assumed for any wired Logitech mouse.
    private static readonly Capability[] WiredBaseline =
    {
        Capability.Dpi,
        Capability.DpiStages,
        Capability.PollingRate,
        Capability.ButtonRemapping,
        Capability.OnboardProfiles,
    };

    // Wired baseline plus wireless/battery capabilities.
    private static readonly Capability[] WirelessBaseline =
    {
        Capability.Dpi,
        Capability.DpiStages,
        Capability.PollingRate,
        Capability.ButtonRemapping,
        Capability.OnboardProfiles,
        Capability.Battery,
        Capability.Sleep,
        Capability.WirelessConnection,
    };

    // A representative, non-exhaustive sample drawn from PLAN.md section 7.
    // Product ids are best-effort placeholders to be refined during Phase 2/3
    // device testing; the catalog is intentionally easy to extend.
    private static readonly IReadOnlyDictionary<ushort, KnownDevice> Known =
        new KnownDevice[]
        {
            new(0xC08B, "G502 HERO", DeviceTier.Tier1, WiredBaseline),
            new(0xC094, "G502 X", DeviceTier.Tier1, WiredBaseline),
            new(0xC099, "G502 X LIGHTSPEED", DeviceTier.Tier1, WirelessBaseline),
            new(0xC08F, "G Pro X Superlight", DeviceTier.Tier1, WirelessBaseline),
            new(0xC09B, "G Pro X Superlight 2", DeviceTier.Tier1, WirelessBaseline),
            new(0xC086, "G903", DeviceTier.Tier2, WirelessBaseline),
            new(0xC087, "G703", DeviceTier.Tier2, WirelessBaseline),
            new(0xC539, "G305", DeviceTier.Tier2, WirelessBaseline),
            new(0xC08D, "G403", DeviceTier.Tier2, WiredBaseline),
        }.ToDictionary(d => d.ProductId);

    /// <summary>
    /// Resolves generic <see cref="DeviceInfo"/> for a device identity. Unknown
    /// Logitech devices are treated as generic Tier 3 with a conservative
    /// capability set; non-Logitech devices are returned as Unknown tier.
    /// </summary>
    public DeviceInfo Resolve(DeviceIdentity identity, DeviceConnection connection)
    {
        if (Known.TryGetValue(identity.ProductId, out var known))
        {
            return new DeviceInfo
            {
                Identity = identity,
                Name = known.Name,
                Tier = known.Tier,
                Connection = connection,
                Capabilities = CapabilityGraph.FromCapabilities(known.Capabilities),
            };
        }

        var generic = connection is DeviceConnection.Lightspeed
            or DeviceConnection.Bluetooth
            or DeviceConnection.Receiver
            ? WirelessBaseline
            : new[] { Capability.Dpi, Capability.PollingRate, Capability.ButtonRemapping };

        return new DeviceInfo
        {
            Identity = identity,
            Name = identity.IsLogitech
                ? $"Logitech Mouse ({identity.ProductId:X4})"
                : $"Unknown Device ({identity.VendorId:X4}:{identity.ProductId:X4})",
            Tier = identity.IsLogitech ? DeviceTier.Tier3 : DeviceTier.Unknown,
            Connection = connection,
            Capabilities = identity.IsLogitech
                ? CapabilityGraph.FromCapabilities(generic)
                : CapabilityGraph.Empty,
        };
    }
}
