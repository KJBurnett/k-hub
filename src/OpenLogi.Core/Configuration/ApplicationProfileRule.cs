namespace OpenLogi.Core.Configuration;

/// <summary>
/// A rule that maps a foreground application executable to a profile so the
/// agent can switch profiles automatically (PLAN.md sections 9 and 12,
/// "Per Application Profiles" / "Automatic profile switching").
/// </summary>
public sealed record ApplicationProfileRule
{
    /// <summary>Creates a rule.</summary>
    /// <param name="executableName">
    /// Executable file name to match, e.g. "chrome.exe". Matching is
    /// case-insensitive.
    /// </param>
    /// <param name="profileId">Id of the profile to activate.</param>
    /// <exception cref="ArgumentException">When either argument is blank.</exception>
    public ApplicationProfileRule(string executableName, string profileId)
    {
        if (string.IsNullOrWhiteSpace(executableName))
        {
            throw new ArgumentException("Executable name must not be empty.", nameof(executableName));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Profile id must not be empty.", nameof(profileId));
        }

        ExecutableName = executableName;
        ProfileId = profileId;
    }

    /// <summary>Executable file name to match.</summary>
    public string ExecutableName { get; init; }

    /// <summary>Id of the profile to activate when the executable is in the foreground.</summary>
    public string ProfileId { get; init; }

    /// <summary>Returns true when <paramref name="foregroundExecutable"/> matches this rule.</summary>
    public bool Matches(string? foregroundExecutable)
        => !string.IsNullOrWhiteSpace(foregroundExecutable)
           && string.Equals(foregroundExecutable, ExecutableName, StringComparison.OrdinalIgnoreCase);
}
