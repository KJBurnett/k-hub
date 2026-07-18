namespace OpenLogi.Core.Configuration;

/// <summary>
/// The kind of a single macro step (PLAN.md section 9, "Macro Engine"). The
/// engine that executes these lives in the agent; the core only models them.
/// Macros stay simple and visual by design (section 10) — no scripting.
/// </summary>
public enum MacroStepKind
{
    /// <summary>Press (and hold) a keyboard key.</summary>
    KeyPress,

    /// <summary>Release a previously pressed keyboard key.</summary>
    KeyRelease,

    /// <summary>Click a mouse button.</summary>
    MouseClick,

    /// <summary>Move the mouse cursor by a relative delta.</summary>
    MouseMove,

    /// <summary>Wait for a fixed delay.</summary>
    Delay,

    /// <summary>Type a literal string of text.</summary>
    Text,

    /// <summary>Emit a media transport key.</summary>
    Media,

    /// <summary>Perform a clipboard operation.</summary>
    Clipboard,

    /// <summary>Launch an application.</summary>
    LaunchApplication,

    /// <summary>Open a URL in the default browser.</summary>
    OpenUrl,
}
