namespace OpenLogi.Core.Devices;

/// <summary>
/// Support tier for a device, mirroring PLAN.md section 7. Tiers are advisory
/// metadata only; actual functionality is always driven by the capability
/// graph, never by the tier.
/// </summary>
public enum DeviceTier
{
    /// <summary>Device is recognised but has no explicit tier assignment.</summary>
    Unknown = 0,

    /// <summary>Fully supported and tested (e.g. G502 family, G Pro X Superlight).</summary>
    Tier1 = 1,

    /// <summary>Expected to work, capability based (e.g. G903, G305, G403).</summary>
    Tier2 = 2,

    /// <summary>Generic Logitech HID++; whatever the device exposes, no guarantees.</summary>
    Tier3 = 3,
}
