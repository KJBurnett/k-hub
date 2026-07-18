namespace OpenLogi.Core.Configuration;

/// <summary>
/// A single immutable macro step. <see cref="Value"/> carries the step-specific
/// payload (key name, text, URL, application path, ...) and <see cref="DelayMs"/>
/// applies to <see cref="MacroStepKind.Delay"/> steps or as an inter-step pause.
/// </summary>
public sealed record MacroStep(MacroStepKind Kind, string? Value = null, int DelayMs = 0)
{
    /// <summary>Creates a delay step.</summary>
    public static MacroStep Delay(int milliseconds)
        => new(MacroStepKind.Delay, DelayMs: milliseconds);

    /// <summary>Creates a text-typing step.</summary>
    public static MacroStep Text(string text) => new(MacroStepKind.Text, text);

    /// <summary>Creates a key-press step.</summary>
    public static MacroStep KeyPress(string key) => new(MacroStepKind.KeyPress, key);

    /// <summary>Creates a key-release step.</summary>
    public static MacroStep KeyRelease(string key) => new(MacroStepKind.KeyRelease, key);
}
