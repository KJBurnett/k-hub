using System.Text.Json;
using Microsoft.Data.Sqlite;
using OpenLogi.Core.Configuration;
using OpenLogi.Storage.Persistence;

namespace OpenLogi.Storage.Repositories;

/// <summary>SQLite-backed <see cref="IProfileRepository"/>.</summary>
public sealed class ProfileRepository : IProfileRepository
{
    private readonly OpenLogiDatabase _database;

    /// <summary>Creates the repository over a database.</summary>
    public ProfileRepository(OpenLogiDatabase database)
        => _database = database ?? throw new ArgumentNullException(nameof(database));

    /// <inheritdoc />
    public Task SaveAsync(Profile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var payload = JsonSerializer.Serialize(ProfileDto.FromDomain(profile), StorageJson.Options);

        return _database.ExecuteAsync(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = """
                INSERT INTO profiles (id, name, is_default, payload_json)
                VALUES ($id, $name, $default, $payload)
                ON CONFLICT (id) DO UPDATE SET
                    name = excluded.name,
                    is_default = excluded.is_default,
                    payload_json = excluded.payload_json;
                """;
            command.Parameters.AddWithValue("$id", profile.Id);
            command.Parameters.AddWithValue("$name", profile.Name);
            command.Parameters.AddWithValue("$default", profile.IsDefault ? 1 : 0);
            command.Parameters.AddWithValue("$payload", payload);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Profile?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _database.ExecuteAsync(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = "SELECT payload_json FROM profiles WHERE id = $id;";
            command.Parameters.AddWithValue("$id", id);
            var payload = (string?)await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return payload is null ? null : Deserialize(payload);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Profile>> GetAllAsync(CancellationToken cancellationToken = default)
        => _database.ExecuteAsync<IReadOnlyList<Profile>>(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = "SELECT payload_json FROM profiles ORDER BY name COLLATE NOCASE;";
            var result = new List<Profile>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                result.Add(Deserialize(reader.GetString(0)));
            }

            return result;
        }, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _database.ExecuteAsync(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = "DELETE FROM profiles WHERE id = $id;";
            command.Parameters.AddWithValue("$id", id);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Profile?> GetDefaultAsync(CancellationToken cancellationToken = default)
        => _database.ExecuteAsync(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = "SELECT payload_json FROM profiles WHERE is_default = 1 LIMIT 1;";
            var payload = (string?)await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            return payload is null ? null : Deserialize(payload);
        }, cancellationToken);

    /// <inheritdoc />
    public Task SetDefaultAsync(string id, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        return _database.ExecuteAsync(async conn =>
        {
            await using var transaction = await conn.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);

            await using (var clear = conn.CreateCommand())
            {
                clear.Transaction = (SqliteTransaction)transaction;
                clear.CommandText = "UPDATE profiles SET is_default = 0 WHERE is_default = 1;";
                await clear.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (var set = conn.CreateCommand())
            {
                set.Transaction = (SqliteTransaction)transaction;
                set.CommandText = "UPDATE profiles SET is_default = 1 WHERE id = $id;";
                set.Parameters.AddWithValue("$id", id);
                if (await set.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false) == 0)
                {
                    throw new KeyNotFoundException($"Profile '{id}' does not exist.");
                }
            }

            await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    private static Profile Deserialize(string payload)
    {
        var dto = JsonSerializer.Deserialize<ProfileDto>(payload, StorageJson.Options)
                  ?? throw new InvalidOperationException("Stored profile payload was empty or invalid.");
        return dto.ToDomain();
    }
}
