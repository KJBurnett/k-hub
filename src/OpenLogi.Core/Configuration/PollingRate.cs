namespace OpenLogi.Core.Configuration;

/// <summary>
/// A report (polling) rate in hertz. Modelled as a validated value type rather
/// than a fixed enum so that devices exposing higher rates (PLAN.md section 9,
/// "Higher polling where hardware supports it") are not artificially capped.
/// </summary>
public readonly record struct PollingRate
{
    /// <summary>The common rates offered by the UI by default.</summary>
    public static readonly IReadOnlyList<PollingRate> Common = new[]
    {
        new PollingRate(125),
        new PollingRate(250),
        new PollingRate(500),
        new PollingRate(1000),
    };

    /// <summary>Creates a polling rate.</summary>
    /// <param name="hz">Rate in hertz; must be positive.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="hz"/> is not positive.</exception>
    public PollingRate(int hz)
    {
        if (hz <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hz), hz, "Polling rate must be positive.");
        }

        Hz = hz;
    }

    /// <summary>The rate in hertz.</summary>
    public int Hz { get; }

    /// <summary>The nominal polling interval in milliseconds.</summary>
    public double IntervalMs => 1000d / Hz;

    /// <inheritdoc />
    public override string ToString() => $"{Hz} Hz";
}
