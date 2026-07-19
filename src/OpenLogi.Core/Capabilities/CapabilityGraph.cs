using System.Collections.Immutable;

namespace OpenLogi.Core.Capabilities;

/// <summary>
/// An immutable snapshot of the capabilities a device advertises, built at
/// runtime when a device connects (PLAN.md section 8). Everything above the
/// device layer queries this graph instead of branching on device model.
/// </summary>
public sealed class CapabilityGraph
{
    private readonly ImmutableHashSet<Capability> _capabilities;

    /// <summary>An empty graph representing a device with no known capabilities.</summary>
    public static readonly CapabilityGraph Empty = new(ImmutableHashSet<Capability>.Empty);

    private CapabilityGraph(ImmutableHashSet<Capability> capabilities)
    {
        _capabilities = capabilities;
    }

    /// <summary>The capabilities contained in this graph.</summary>
    public IReadOnlyCollection<Capability> Capabilities => _capabilities;

    /// <summary>Returns true when the device advertises <paramref name="capability"/>.</summary>
    public bool Has(Capability capability) => _capabilities.Contains(capability);

    /// <summary>Returns true when the device advertises every requested capability.</summary>
    public bool HasAll(params Capability[] capabilities) => capabilities.All(_capabilities.Contains);

    /// <summary>Returns true when the device advertises at least one requested capability.</summary>
    public bool HasAny(params Capability[] capabilities) => capabilities.Any(_capabilities.Contains);

    /// <summary>Creates a builder for assembling a capability graph.</summary>
    public static Builder Create() => new();

    /// <summary>Creates a graph from an existing set of capabilities.</summary>
    public static CapabilityGraph FromCapabilities(IEnumerable<Capability> capabilities)
        => new(capabilities.ToImmutableHashSet());

    /// <summary>Fluent builder for a <see cref="CapabilityGraph"/>.</summary>
    public sealed class Builder
    {
        private readonly HashSet<Capability> _capabilities = new();

        /// <summary>Adds a capability to the graph being built.</summary>
        public Builder Add(Capability capability)
        {
            _capabilities.Add(capability);
            return this;
        }

        /// <summary>Adds a capability only when <paramref name="condition"/> is true.</summary>
        public Builder AddIf(bool condition, Capability capability)
        {
            if (condition)
            {
                _capabilities.Add(capability);
            }

            return this;
        }

        /// <summary>Builds the immutable capability graph.</summary>
        public CapabilityGraph Build() => new(_capabilities.ToImmutableHashSet());
    }
}
