namespace OpenLogi.Core.Configuration;

/// <summary>
/// An immutable action bound to a button. The <see cref="Target"/> carries the
/// action-specific payload (e.g. a key name, an application path, or a macro
/// id) so a single model covers every <see cref="ButtonActionKind"/> without a
/// combinatorial class hierarchy.
/// </summary>
public sealed record ButtonAction(ButtonActionKind Kind, string? Target = null)
{
    /// <summary>An action that does nothing.</summary>
    public static readonly ButtonAction None = new(ButtonActionKind.None);

    /// <summary>Creates an action that runs the macro with the given id.</summary>
    public static ButtonAction Macro(string macroId) => new(ButtonActionKind.Macro, macroId);

    /// <summary>Creates an action that launches the given application path.</summary>
    public static ButtonAction LaunchApplication(string path)
        => new(ButtonActionKind.LaunchApplication, path);

    /// <summary>Creates an action that emits the given keyboard key or chord.</summary>
    public static ButtonAction Keyboard(string keys) => new(ButtonActionKind.Keyboard, keys);
}
