using OpenLogi.Core.Configuration;

namespace OpenLogi.Core.Tests;

public class DpiSettingsTests
{
    [Fact]
    public void ActiveStage_reflects_active_index()
    {
        var settings = new DpiSettings(
            new[] { new DpiStage("Low", 400), new DpiStage("High", 3200) }, 1);

        Assert.Equal(3200, settings.ActiveStage!.Dpi);
    }

    [Fact]
    public void AddStage_appends_without_mutating_original()
    {
        var original = DpiSettings.Default;
        var updated = original.AddStage(new DpiStage("Sniper", 400));

        Assert.Single(original.Stages);
        Assert.Equal(2, updated.Stages.Count);
    }

    [Fact]
    public void RemoveStage_reindexes_active_stage()
    {
        var settings = new DpiSettings(
            new[] { new DpiStage("A", 400), new DpiStage("B", 800), new DpiStage("C", 1600) }, 2);

        var updated = settings.RemoveStage(0);

        Assert.Equal(2, updated.Stages.Count);
        Assert.Equal("C", updated.ActiveStage!.Name);
    }

    [Fact]
    public void Constructor_rejects_out_of_range_active_index()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new DpiSettings(new[] { new DpiStage("A", 400) }, 5));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(40000)]
    public void DpiStage_rejects_out_of_range_dpi(int dpi)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DpiStage("Stage", dpi));
    }
}
