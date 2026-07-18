using System.Collections.Concurrent;

namespace OpenLogi.Hid.Testing;

/// <summary>
/// An in-memory <see cref="IHidDevice"/> used for automated testing and for a
/// hardware-free demo mode (PLAN.md section 20). Writes are recorded, queued
/// input reports are returned from <see cref="ReadAsync"/>, and feature reports
/// are stored so round-trips can be asserted.
/// </summary>
public sealed class MockHidDevice : IHidDevice
{
    private readonly ConcurrentQueue<byte[]> _inputReports = new();
    private readonly List<byte[]> _writtenReports = new();
    private byte[] _featureReport = Array.Empty<byte>();
    private readonly object _gate = new();

    /// <summary>Creates a mock device from a descriptor.</summary>
    public MockHidDevice(HidDeviceDescriptor descriptor) => Descriptor = descriptor;

    /// <inheritdoc />
    public HidDeviceDescriptor Descriptor { get; }

    /// <inheritdoc />
    public bool IsOpen { get; private set; } = true;

    /// <summary>A snapshot of every output report written to this device.</summary>
    public IReadOnlyList<byte[]> WrittenReports
    {
        get
        {
            lock (_gate)
            {
                return _writtenReports.ToArray();
            }
        }
    }

    /// <summary>Queues an input report to be returned by a future read.</summary>
    public void EnqueueInputReport(params byte[] report)
    {
        ArgumentNullException.ThrowIfNull(report);
        _inputReports.Enqueue(report);
    }

    /// <inheritdoc />
    public ValueTask<int> WriteAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfClosed();
        lock (_gate)
        {
            _writtenReports.Add(report.ToArray());
        }

        return ValueTask.FromResult(report.Length);
    }

    /// <inheritdoc />
    public ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfClosed();
        if (!_inputReports.TryDequeue(out var report))
        {
            return ValueTask.FromResult(0);
        }

        var count = Math.Min(report.Length, buffer.Length);
        report.AsSpan(0, count).CopyTo(buffer.Span);
        return ValueTask.FromResult(count);
    }

    /// <inheritdoc />
    public ValueTask SetFeatureAsync(ReadOnlyMemory<byte> report, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfClosed();
        _featureReport = report.ToArray();
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public ValueTask<int> GetFeatureAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfClosed();
        var count = Math.Min(_featureReport.Length, buffer.Length);
        _featureReport.AsSpan(0, count).CopyTo(buffer.Span);
        return ValueTask.FromResult(count);
    }

    /// <inheritdoc />
    public void Dispose() => IsOpen = false;

    private void ThrowIfClosed()
    {
        if (!IsOpen)
        {
            throw new ObjectDisposedException(nameof(MockHidDevice));
        }
    }
}
