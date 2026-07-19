using System.Collections.Concurrent;

namespace OpenLogi.Core.Events;

/// <summary>
/// A thread-safe, synchronous <see cref="IEventBus"/> implementation. Handlers
/// are invoked on the publishing thread. A handler that throws does not prevent
/// the remaining handlers from running; the aggregated failures are rethrown so
/// they are never silently swallowed.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        if (!_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            return;
        }

        Delegate[] snapshot;
        lock (handlers)
        {
            snapshot = handlers.ToArray();
        }

        List<Exception>? failures = null;
        foreach (var handler in snapshot)
        {
            try
            {
                ((Action<TEvent>)handler)(domainEvent);
            }
            catch (Exception ex)
            {
                (failures ??= new List<Exception>()).Add(ex);
            }
        }

        if (failures is { Count: > 0 })
        {
            throw new AggregateException(
                "One or more event handlers threw an exception.", failures);
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var handlers = _handlers.GetOrAdd(typeof(TEvent), _ => new List<Delegate>());
        lock (handlers)
        {
            handlers.Add(handler);
        }

        return new Subscription(() =>
        {
            lock (handlers)
            {
                handlers.Remove(handler);
            }
        });
    }

    private sealed class Subscription : IDisposable
    {
        private Action? _unsubscribe;

        public Subscription(Action unsubscribe) => _unsubscribe = unsubscribe;

        public void Dispose()
        {
            Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
        }
    }
}
