using OpenLogi.Core.Configuration;

namespace OpenLogi.Storage.Repositories;

/// <summary>
/// Persists application-to-profile rules used for automatic per-application
/// profile switching (PLAN.md sections 9 and 12).
/// </summary>
public interface IApplicationRuleRepository
{
    /// <summary>Inserts or updates a rule keyed by executable name.</summary>
    Task UpsertAsync(ApplicationProfileRule rule, CancellationToken cancellationToken = default);

    /// <summary>Returns every rule.</summary>
    Task<IReadOnlyList<ApplicationProfileRule>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Removes the rule for the given executable, if present.</summary>
    Task DeleteAsync(string executableName, CancellationToken cancellationToken = default);
}

/// <summary>SQLite-backed <see cref="IApplicationRuleRepository"/>.</summary>
public sealed class ApplicationRuleRepository : IApplicationRuleRepository
{
    private readonly OpenLogiDatabase _database;

    /// <summary>Creates the repository over a database.</summary>
    public ApplicationRuleRepository(OpenLogiDatabase database)
        => _database = database ?? throw new ArgumentNullException(nameof(database));

    /// <inheritdoc />
    public Task UpsertAsync(ApplicationProfileRule rule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return _database.ExecuteAsync(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = """
                INSERT INTO applications (executable, profile_id) VALUES ($exe, $profile)
                ON CONFLICT (executable) DO UPDATE SET profile_id = excluded.profile_id;
                """;
            command.Parameters.AddWithValue("$exe", rule.ExecutableName);
            command.Parameters.AddWithValue("$profile", rule.ProfileId);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ApplicationProfileRule>> GetAllAsync(CancellationToken cancellationToken = default)
        => _database.ExecuteAsync<IReadOnlyList<ApplicationProfileRule>>(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = "SELECT executable, profile_id FROM applications ORDER BY executable COLLATE NOCASE;";
            var result = new List<ApplicationProfileRule>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result.Add(new ApplicationProfileRule(reader.GetString(0), reader.GetString(1)));
            }

            return result;
        }, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(string executableName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executableName);
        return _database.ExecuteAsync(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = "DELETE FROM applications WHERE executable = $exe;";
            command.Parameters.AddWithValue("$exe", executableName);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }
}
