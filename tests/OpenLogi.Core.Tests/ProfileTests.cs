using OpenLogi.Core.Configuration;

namespace OpenLogi.Core.Tests;

public class ProfileTests
{
    [Fact]
    public void Duplicate_clears_default_and_changes_identity()
    {
        var original = new Profile("id1", "Gaming") { IsDefault = true };

        var copy = original.Duplicate("id2", "Gaming copy");

        Assert.Equal("id2", copy.Id);
        Assert.Equal("Gaming copy", copy.Name);
        Assert.False(copy.IsDefault);
        Assert.True(original.IsDefault);
    }

    [Fact]
    public void Rename_changes_name_only()
    {
        var profile = new Profile("id1", "Old");

        var renamed = profile.Rename("New");

        Assert.Equal("New", renamed.Name);
        Assert.Equal("id1", renamed.Id);
    }

    [Fact]
    public void Constructor_rejects_blank_name()
    {
        Assert.Throws<ArgumentException>(() => new Profile("id", "  "));
    }
}

public class ApplicationProfileRuleTests
{
    [Fact]
    public void Matches_is_case_insensitive()
    {
        var rule = new ApplicationProfileRule("Chrome.exe", "p1");

        Assert.True(rule.Matches("chrome.exe"));
        Assert.False(rule.Matches("firefox.exe"));
        Assert.False(rule.Matches(null));
    }
}
