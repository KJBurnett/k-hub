using OpenLogi.Core.Capabilities;

namespace OpenLogi.Core.Tests;

public class CapabilityGraphTests
{
    [Fact]
    public void Builder_adds_and_queries_capabilities()
    {
        var graph = CapabilityGraph.Create()
            .Add(Capability.Dpi)
            .Add(Capability.PollingRate)
            .Build();

        Assert.True(graph.Has(Capability.Dpi));
        Assert.True(graph.HasAll(Capability.Dpi, Capability.PollingRate));
        Assert.False(graph.Has(Capability.Battery));
    }

    [Fact]
    public void AddIf_only_adds_when_condition_is_true()
    {
        var graph = CapabilityGraph.Create()
            .AddIf(true, Capability.Dpi)
            .AddIf(false, Capability.Battery)
            .Build();

        Assert.True(graph.Has(Capability.Dpi));
        Assert.False(graph.Has(Capability.Battery));
    }

    [Fact]
    public void HasAny_returns_true_when_at_least_one_present()
    {
        var graph = CapabilityGraph.FromCapabilities(new[] { Capability.Sleep });

        Assert.True(graph.HasAny(Capability.Battery, Capability.Sleep));
        Assert.False(graph.HasAny(Capability.Battery, Capability.PollingRate));
    }

    [Fact]
    public void Empty_graph_has_no_capabilities()
    {
        Assert.Empty(CapabilityGraph.Empty.Capabilities);
    }
}
