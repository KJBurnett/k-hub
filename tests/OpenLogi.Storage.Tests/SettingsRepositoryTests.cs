using OpenLogi.Core.Configuration;
using OpenLogi.Logging;
using OpenLogi.Storage;
using OpenLogi.Storage.Repositories;

namespace OpenLogi.Storage.Tests;

public class SettingsRepositoryTests
{
    private static readonly IAppLoggerFactory LoggerFactory = new AppLoggerFactory(LogLevel.Warning);

    private static async Task<OpenLogiDatabase> CreateDatabaseAsync()
    {
        var database = OpenLogiDatabase.InMemory(LoggerFactory);
        await database.InitializeAsync();
        return database;
    }

    [Fact]
    public async Task GetAppSettings_returns_defaults_when_empty()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new SettingsRepository(database);

        var settings = await repository.GetAppSettingsAsync();

        Assert.Equal(AppSettings.Default, settings);
        Assert.False(settings.TelemetryEnabled);
    }

    [Fact]
    public async Task AppSettings_round_trip_preserves_values()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new SettingsRepository(database);
        var settings = new AppSettings
        {
            Theme = AppTheme.Dark,
            StartWithSystem = false,
            MinimizeToTray = false,
            LogVerbosity = LogVerbosity.Debug,
        };

        await repository.SaveAppSettingsAsync(settings);
        var loaded = await repository.GetAppSettingsAsync();

        Assert.Equal(AppTheme.Dark, loaded.Theme);
        Assert.False(loaded.StartWithSystem);
        Assert.Equal(LogVerbosity.Debug, loaded.LogVerbosity);
        Assert.False(loaded.TelemetryEnabled);
    }

    [Fact]
    public async Task Key_value_round_trips_and_upserts()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new SettingsRepository(database);

        await repository.SetValueAsync("k", "v1");
        await repository.SetValueAsync("k", "v2");

        Assert.Equal("v2", await repository.GetValueAsync("k"));
        Assert.Null(await repository.GetValueAsync("missing"));
    }

    [Fact]
    public async Task InitializeAsync_allows_concurrent_callers()
    {
        await using var database = OpenLogiDatabase.InMemory(LoggerFactory);

        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => database.InitializeAsync()));

        var settings = await new SettingsRepository(database).GetAppSettingsAsync();

        Assert.Equal(AppSettings.Default, settings);
    }
}
