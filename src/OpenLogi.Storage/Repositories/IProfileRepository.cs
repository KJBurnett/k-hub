using OpenLogi.Core.Configuration;

namespace OpenLogi.Storage.Repositories;

/// <summary>Persists local-only profiles (PLAN.md section 9, "Profiles").</summary>
public interface IProfileRepository
{
    /// <summary>Inserts or updates a profile.</summary>
    Task SaveAsync(Profile profile, CancellationToken cancellationToken = default);

    /// <summary>Returns the profile with the given id, or null if it does not exist.</summary>
    Task<Profile?> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Returns every stored profile, ordered by name.</summary>
    Task<IReadOnlyList<Profile>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Deletes the profile with the given id.</summary>
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>Returns the default profile, or null when none is marked default.</summary>
    Task<Profile?> GetDefaultAsync(CancellationToken cancellationToken = default);

    /// <summary>Marks the given profile as the default, clearing the flag on all others.</summary>
    Task SetDefaultAsync(string id, CancellationToken cancellationToken = default);
}
