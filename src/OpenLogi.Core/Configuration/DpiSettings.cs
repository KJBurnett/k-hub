using System.Collections.Immutable;

namespace OpenLogi.Core.Configuration;

/// <summary>
/// An immutable set of DPI stages plus the index of the active stage. Mutating
/// helpers return new instances, keeping configuration models immutable
/// (Appendix A #6). Empty settings are allowed for devices with no DPI control.
/// </summary>
public sealed record DpiSettings
{
    /// <summary>Default DPI settings offering a single 800 DPI stage.</summary>
    public static readonly DpiSettings Default = new(
        new[] { new DpiStage("Default", 800) }, 0);

    /// <summary>Creates DPI settings.</summary>
    /// <param name="stages">Ordered DPI stages.</param>
    /// <param name="activeStageIndex">Index of the active stage.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When the active index falls outside a non-empty stage list.
    /// </exception>
    public DpiSettings(IEnumerable<DpiStage> stages, int activeStageIndex)
    {
        Stages = stages.ToImmutableList();

        if (Stages.Count == 0)
        {
            ActiveStageIndex = 0;
        }
        else if (activeStageIndex < 0 || activeStageIndex >= Stages.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(activeStageIndex), activeStageIndex,
                "Active stage index is outside the range of stages.");
        }
        else
        {
            ActiveStageIndex = activeStageIndex;
        }
    }

    /// <summary>The ordered DPI stages.</summary>
    public ImmutableList<DpiStage> Stages { get; init; }

    /// <summary>Index of the currently active stage.</summary>
    public int ActiveStageIndex { get; init; }

    /// <summary>The active stage, or null when no stages are configured.</summary>
    public DpiStage? ActiveStage => Stages.Count == 0 ? null : Stages[ActiveStageIndex];

    /// <summary>Returns new settings with an additional stage appended.</summary>
    public DpiSettings AddStage(DpiStage stage)
        => this with { Stages = Stages.Add(stage) };

    /// <summary>Returns new settings with the stage at <paramref name="index"/> removed.</summary>
    public DpiSettings RemoveStage(int index)
    {
        if (index < 0 || index >= Stages.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var stages = Stages.RemoveAt(index);
        var active = Math.Clamp(
            ActiveStageIndex > index ? ActiveStageIndex - 1 : ActiveStageIndex,
            0,
            Math.Max(0, stages.Count - 1));
        return new DpiSettings(stages, active);
    }

    /// <summary>Returns new settings with a different active stage selected.</summary>
    public DpiSettings WithActiveStage(int index) => new(Stages, index);
}
