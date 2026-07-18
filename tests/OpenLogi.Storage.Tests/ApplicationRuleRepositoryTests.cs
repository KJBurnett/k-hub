using OpenLogi.Core.Configuration;
using OpenLogi.Logging;
using OpenLogi.Storage;
using OpenLogi.Storage.Repositories;

namespace OpenLogi.Storage.Tests;

public class ApplicationRuleRepositoryTests
{
    private static readonly IAppLoggerFactory LoggerFactory = new AppLoggerFactory(LogLevel.Warning);

    private static async Task<OpenLogiDatabase> CreateDatabaseAsync()
    {
        var database = OpenLogiDatabase.InMemory(LoggerFactory);
        await database.InitializeAsync();
        // Application rules reference profiles via a foreign key, so seed the
        // profiles they point at first.
        var profiles = new ProfileRepository(database);
        await profiles.SaveAsync(new Profile("p1", "Profile 1"));
        await profiles.SaveAsync(new Profile("p2", "Profile 2"));
        return database;
    }

    [Fact]
    public async Task Upsert_replaces_existing_rule_for_executable()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new ApplicationRuleRepository(database);

        await repository.UpsertAsync(new ApplicationProfileRule("game.exe", "p1"));
        await repository.UpsertAsync(new ApplicationProfileRule("game.exe", "p2"));

        var rule = Assert.Single(await repository.GetAllAsync());
        Assert.Equal("p2", rule.ProfileId);
    }

    [Fact]
    public async Task GetAll_returns_rules_ordered_by_executable()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new ApplicationRuleRepository(database);
        await repository.UpsertAsync(new ApplicationProfileRule("zeta.exe", "p1"));
        await repository.UpsertAsync(new ApplicationProfileRule("alpha.exe", "p2"));

        var all = await repository.GetAllAsync();

        Assert.Equal(new[] { "alpha.exe", "zeta.exe" }, all.Select(r => r.ExecutableName));
    }

    [Fact]
    public async Task Delete_removes_rule()
    {
        await using var database = await CreateDatabaseAsync();
        var repository = new ApplicationRuleRepository(database);
        await repository.UpsertAsync(new ApplicationProfileRule("game.exe", "p1"));

        await repository.DeleteAsync("game.exe");

        Assert.Empty(await repository.GetAllAsync());
    }
}
