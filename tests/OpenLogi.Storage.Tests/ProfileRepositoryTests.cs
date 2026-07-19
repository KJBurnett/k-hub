using OpenLogi.Core.Configuration;
using OpenLogi.Logging;
using OpenLogi.Storage;
using OpenLogi.Storage.Repositories;

namespace OpenLogi.Storage.Tests;

public class ProfileRepositoryTests
{
    private static readonly IAppLoggerFactory LoggerFactory = new AppLoggerFactory(LogLevel.Warning);

    private static async Task<OpenLogiDatabase> CreateDatabaseAsync()
    {
        var database = OpenLogiDatabase.InMemory(LoggerFactory);
        await database.InitializeAsync();
        return database;
    }

    private static Profile SampleProfile(string id, string name) => new(id, name)
    {
        Dpi = new DpiSettings(
            new[] { new DpiStage("Low", 800), new DpiStage("High", 3200) }, 1),
        PollingRate = new PollingRate(500),
        ButtonMappings = System.Collections.Immutable.ImmutableList.Create(
            new ButtonMapping(0, ButtonAction.Keyboard("Ctrl+C"))),
    };

    [Fact]
    public async Task Save_then_get_round_trips_profile_details()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new ProfileRepository(database);
        var profile = SampleProfile("p1", "Gaming");

        await repository.SaveAsync(profile);
        var loaded = await repository.GetAsync("p1");

        Assert.NotNull(loaded);
        Assert.Equal("Gaming", loaded!.Name);
        Assert.Equal(2, loaded.Dpi.Stages.Count);
        Assert.Equal(3200, loaded.Dpi.ActiveStage!.Dpi);
        Assert.Equal(500, loaded.PollingRate.Hz);
        Assert.Single(loaded.ButtonMappings);
    }

    [Fact]
    public async Task GetAll_returns_profiles_ordered_by_name()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new ProfileRepository(database);
        await repository.SaveAsync(SampleProfile("p1", "Zebra"));
        await repository.SaveAsync(SampleProfile("p2", "Alpha"));

        var all = await repository.GetAllAsync();

        Assert.Equal(new[] { "Alpha", "Zebra" }, all.Select(p => p.Name));
    }

    [Fact]
    public async Task Delete_removes_profile()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new ProfileRepository(database);
        await repository.SaveAsync(SampleProfile("p1", "Gaming"));

        await repository.DeleteAsync("p1");

        Assert.Null(await repository.GetAsync("p1"));
    }

    [Fact]
    public async Task SetDefault_marks_single_profile_as_default()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new ProfileRepository(database);
        await repository.SaveAsync(SampleProfile("p1", "A"));
        await repository.SaveAsync(SampleProfile("p2", "B"));

        await repository.SetDefaultAsync("p1");
        await repository.SetDefaultAsync("p2");

        var def = await repository.GetDefaultAsync();
        Assert.Equal("p2", def!.Id);
        Assert.False((await repository.GetAsync("p1"))!.IsDefault);
    }

    [Fact]
    public async Task SetDefault_for_missing_profile_preserves_existing_default()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new ProfileRepository(database);
        await repository.SaveAsync(SampleProfile("p1", "A"));
        await repository.SetDefaultAsync("p1");

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.SetDefaultAsync("missing"));

        Assert.Equal("p1", (await repository.GetDefaultAsync())!.Id);
    }
}
