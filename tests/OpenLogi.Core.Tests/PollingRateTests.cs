using OpenLogi.Core.Configuration;

namespace OpenLogi.Core.Tests;

public class PollingRateTests
{
    [Fact]
    public void Interval_is_derived_from_hertz()
    {
        Assert.Equal(1d, new PollingRate(1000).IntervalMs, 3);
    }

    [Fact]
    public void Constructor_rejects_non_positive_rate()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PollingRate(0));
    }

    [Fact]
    public void Common_rates_are_exposed()
    {
        Assert.Contains(new PollingRate(1000), PollingRate.Common);
        Assert.Equal(4, PollingRate.Common.Count);
    }
}
