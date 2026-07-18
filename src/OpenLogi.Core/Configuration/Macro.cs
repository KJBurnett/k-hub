using System.Collections.Immutable;

namespace OpenLogi.Core.Configuration;

/// <summary>
/// A named, ordered sequence of macro steps. Macros can optionally repeat a
/// fixed number of times (PLAN.md section 9, "Repeat").
/// </summary>
public sealed record Macro
{
    /// <summary>Creates a macro.</summary>
    /// <param name="id">Stable unique identifier.</param>
    /// <param name="name">Display name.</param>
    /// <param name="steps">Ordered steps.</param>
    /// <param name="repeatCount">How many times to run; 1 means run once.</param>
    /// <exception cref="ArgumentException">When <paramref name="name"/> is blank.</exception>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="repeatCount"/> is below 1.</exception>
    public Macro(string id, string name, IEnumerable<MacroStep> steps, int repeatCount = 1)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Macro name must not be empty.", nameof(name));
        }

        if (repeatCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(repeatCount), repeatCount, "Repeat count must be at least 1.");
        }

        Id = id;
        Name = name;
        Steps = steps.ToImmutableList();
        RepeatCount = repeatCount;
    }

    /// <summary>Stable unique identifier.</summary>
    public string Id { get; init; }

    /// <summary>Display name.</summary>
    public string Name { get; init; }

    /// <summary>Ordered steps.</summary>
    public ImmutableList<MacroStep> Steps { get; init; }

    /// <summary>Number of times the macro runs when triggered.</summary>
    public int RepeatCount { get; init; }
}
