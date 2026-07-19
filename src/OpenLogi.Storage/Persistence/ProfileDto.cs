using System.Collections.Immutable;
using OpenLogi.Core.Configuration;

namespace OpenLogi.Storage.Persistence;

/// <summary>
/// Flat, serializer-friendly representation of a <see cref="Profile"/> used only
/// for JSON persistence. Keeping storage DTOs separate from the immutable domain
/// models means the domain never has to compromise its shape for a serializer.
/// </summary>
internal sealed class ProfileDto
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public List<DpiStageDto> DpiStages { get; set; } = new();

    public int ActiveDpiStageIndex { get; set; }

    public int PollingRateHz { get; set; } = 1000;

    public List<ButtonMappingDto> Buttons { get; set; } = new();

    public static ProfileDto FromDomain(Profile profile) => new()
    {
        Id = profile.Id,
        Name = profile.Name,
        IsDefault = profile.IsDefault,
        DpiStages = profile.Dpi.Stages
            .Select(s => new DpiStageDto { Name = s.Name, Dpi = s.Dpi })
            .ToList(),
        ActiveDpiStageIndex = profile.Dpi.ActiveStageIndex,
        PollingRateHz = profile.PollingRate.Hz,
        Buttons = profile.ButtonMappings
            .Select(b => new ButtonMappingDto
            {
                ButtonId = b.ButtonId,
                ActionKind = (int)b.Action.Kind,
                Target = b.Action.Target,
            })
            .ToList(),
    };

    public Profile ToDomain()
    {
        var dpi = DpiStages.Count == 0
            ? DpiSettings.Default
            : new DpiSettings(
                DpiStages.Select(s => new DpiStage(s.Name, s.Dpi)),
                Math.Clamp(ActiveDpiStageIndex, 0, DpiStages.Count - 1));

        var buttons = Buttons
            .Select(b => new ButtonMapping(b.ButtonId, new ButtonAction((ButtonActionKind)b.ActionKind, b.Target)))
            .ToImmutableList();

        return new Profile(Id, Name)
        {
            IsDefault = IsDefault,
            Dpi = dpi,
            PollingRate = new PollingRate(PollingRateHz),
            ButtonMappings = buttons,
        };
    }
}

internal sealed class DpiStageDto
{
    public string Name { get; set; } = string.Empty;

    public int Dpi { get; set; }
}

internal sealed class ButtonMappingDto
{
    public int ButtonId { get; set; }

    public int ActionKind { get; set; }

    public string? Target { get; set; }
}
