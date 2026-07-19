namespace OpenLogi.Core.Configuration;

/// <summary>
/// Binds a physical button (identified by a stable zero-based id reported by
/// the device layer) to an action.
/// </summary>
public sealed record ButtonMapping(int ButtonId, ButtonAction Action)
{
    /// <summary>Returns a copy of this mapping bound to a different action.</summary>
    public ButtonMapping WithAction(ButtonAction action) => this with { Action = action };
}
