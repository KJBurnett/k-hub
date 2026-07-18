using System.Collections.Immutable;

namespace OpenLogi.Core.Configuration;

/// <summary>
/// A local-only profile (PLAN.md section 9, "Profiles"). Profiles are stored on
/// the machine, never in the cloud. A profile bundles DPI, polling rate and
/// button mappings, and can be duplicated, renamed, imported and exported. This
/// model is immutable; edits produce a new instance.
/// </summary>
public sealed record Profile
{
    /// <summary>Creates a profile.</summary>
    /// <param name="id">Stable unique identifier.</param>
    /// <param name="name">Display name.</param>
    /// <exception cref="ArgumentException">When <paramref name="name"/> is blank.</exception>
    public Profile(string id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Profile name must not be empty.", nameof(name));
        }

        Id = id;
        Name = name;
    }

    /// <summary>Stable unique identifier.</summary>
    public string Id { get; init; }

    /// <summary>Display name.</summary>
    public string Name { get; init; }

    /// <summary>Whether this is the default profile applied when nothing else matches.</summary>
    public bool IsDefault { get; init; }

    /// <summary>DPI configuration for this profile.</summary>
    public DpiSettings Dpi { get; init; } = DpiSettings.Default;

    /// <summary>Polling rate for this profile.</summary>
    public PollingRate PollingRate { get; init; } = new(1000);

    /// <summary>Button mappings for this profile, keyed by button id ordering.</summary>
    public ImmutableList<ButtonMapping> ButtonMappings { get; init; } =
        ImmutableList<ButtonMapping>.Empty;

    /// <summary>
    /// Returns a duplicate of this profile with a new id and name, never marked
    /// as the default (PLAN.md section 9, "Duplicate").
    /// </summary>
    public Profile Duplicate(string newId, string newName)
        => this with { Id = newId, Name = newName, IsDefault = false };

    /// <summary>Returns a copy of this profile renamed to <paramref name="newName"/>.</summary>
    public Profile Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new ArgumentException("Profile name must not be empty.", nameof(newName));
        }

        return this with { Name = newName };
    }
}
