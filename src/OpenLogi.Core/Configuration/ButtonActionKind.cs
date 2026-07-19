namespace OpenLogi.Core.Configuration;

/// <summary>
/// The kind of action bound to a physical button (PLAN.md section 9,
/// "Button Mapping"). New kinds can be added over time, and the future plugin
/// system (section 24) can contribute additional actions without changing the
/// core mapping model.
/// </summary>
public enum ButtonActionKind
{
    /// <summary>No action; the button is disabled.</summary>
    None = 0,

    /// <summary>Emit a standard mouse button.</summary>
    MouseButton,

    /// <summary>Emit a keyboard key or chord.</summary>
    Keyboard,

    /// <summary>Emit a media transport key (play/pause/next/etc.).</summary>
    Media,

    /// <summary>Adjust system volume.</summary>
    Volume,

    /// <summary>Launch an application.</summary>
    LaunchApplication,

    /// <summary>Trigger a Windows shortcut / system command.</summary>
    SystemShortcut,

    /// <summary>Perform a clipboard operation (copy/paste/etc.).</summary>
    Clipboard,

    /// <summary>Perform browser navigation (back/forward/etc.).</summary>
    BrowserNavigation,

    /// <summary>Run a macro identified by <see cref="ButtonAction.Target"/>.</summary>
    Macro,
}
