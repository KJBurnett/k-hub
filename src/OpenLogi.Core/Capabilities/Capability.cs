namespace OpenLogi.Core.Capabilities;

/// <summary>
/// A single feature a device may expose. OpenLogi is capability-driven, not
/// model-driven (PLAN.md sections 8, 14 and Appendix A #1): the UI and agent
/// react to the set of capabilities a device advertises rather than to a
/// hard-coded device model. Unknown devices degrade gracefully by simply
/// advertising fewer capabilities.
/// </summary>
public enum Capability
{
    /// <summary>Report and change the current DPI / sensitivity.</summary>
    Dpi,

    /// <summary>Multiple selectable DPI stages that can be added, renamed and removed.</summary>
    DpiStages,

    /// <summary>Change the USB / wireless report (polling) rate.</summary>
    PollingRate,

    /// <summary>Report battery level and charging state (wireless devices).</summary>
    Battery,

    /// <summary>Remap physical buttons to actions.</summary>
    ButtonRemapping,

    /// <summary>Store profiles in onboard device memory.</summary>
    OnboardProfiles,

    /// <summary>Enter and report a low-power sleep state.</summary>
    Sleep,

    /// <summary>Operate over a wireless (LIGHTSPEED / Bluetooth / receiver) link.</summary>
    WirelessConnection,
}
