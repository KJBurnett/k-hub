namespace OpenLogi.Core.Configuration;

/// <summary>
/// A single named DPI stage (PLAN.md section 9). Stages can be added, renamed
/// and removed by the user; the value is a raw DPI figure.
/// </summary>
public sealed record DpiStage
{
    /// <summary>Lowest DPI OpenLogi will accept for a stage.</summary>
    public const int MinDpi = 50;

    /// <summary>Highest DPI OpenLogi will accept for a stage.</summary>
    public const int MaxDpi = 32000;

    /// <summary>Creates a DPI stage.</summary>
    /// <param name="name">Display name for the stage.</param>
    /// <param name="dpi">DPI value, clamped to the supported range.</param>
    /// <exception cref="ArgumentException">When <paramref name="name"/> is blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="dpi"/> is out of range.</exception>
    public DpiStage(string name, int dpi)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Stage name must not be empty.", nameof(name));
        }

        if (dpi is < MinDpi or > MaxDpi)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dpi), dpi, $"DPI must be between {MinDpi} and {MaxDpi}.");
        }

        Name = name;
        Dpi = dpi;
    }

    /// <summary>Display name for the stage.</summary>
    public string Name { get; init; }

    /// <summary>DPI value.</summary>
    public int Dpi { get; init; }
}
