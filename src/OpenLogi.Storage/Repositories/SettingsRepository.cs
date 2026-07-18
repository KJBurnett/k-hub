using System.Text.Json;
using OpenLogi.Core.Configuration;

namespace OpenLogi.Storage.Repositories;

/// <summary>Reads and writes application settings and arbitrary key/value pairs.</summary>
public interface ISettingsRepository
{
    /// <summary>Returns the stored application settings, or defaults if none saved.</summary>
    Task<AppSettings> GetAppSettingsAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists the application settings.</summary>
    Task SaveAppSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default);

    /// <summary>Returns a raw string value for a key, or null when absent.</summary>
    Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Sets a raw string value for a key.</summary>
    Task SetValueAsync(string key, string value, CancellationToken cancellationToken = default);
}

/// <summary>SQLite-backed <see cref="ISettingsRepository"/> using the settings key/value table.</summary>
public sealed class SettingsRepository : ISettingsRepository
{
    private const string AppSettingsKey = "app.settings";

    private readonly OpenLogiDatabase _database;

    /// <summary>Creates the repository over a database.</summary>
    public SettingsRepository(OpenLogiDatabase database)
        => _database = database ?? throw new ArgumentNullException(nameof(database));

    /// <inheritdoc />
    public async Task<AppSettings> GetAppSettingsAsync(CancellationToken cancellationToken = default)
    {
        var json = await GetValueAsync(AppSettingsKey, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json))
        {
            return AppSettings.Default;
        }

        return JsonSerializer.Deserialize<AppSettings>(json) ?? AppSettings.Default;
    }

    /// <inheritdoc />
    public Task SaveAppSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var json = JsonSerializer.Serialize(settings);
        return SetValueAsync(AppSettingsKey, json, cancellationToken);
    }

    /// <inheritdoc />
    public Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return _database.ExecuteAsync(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = "SELECT value FROM settings WHERE key = $key;";
            command.Parameters.AddWithValue("$key", key);
            return (string?)await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task SetValueAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);
        return _database.ExecuteAsync(async conn =>
        {
            await using var command = conn.CreateCommand();
            command.CommandText = """
                INSERT INTO settings (key, value) VALUES ($key, $value)
                ON CONFLICT (key) DO UPDATE SET value = excluded.value;
                """;
            command.Parameters.AddWithValue("$key", key);
            command.Parameters.AddWithValue("$value", value);
            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }
}
