namespace OpenLogi.Core.Events;

/// <summary>
/// A lightweight, in-process publish/subscribe bus used to decouple subsystems
/// (device layer, agent, UI). It is an injectable instance rather than a static
/// singleton to keep global state minimal (Appendix A #3).
/// </summary>
public interface IEventBus
{
    /// <summary>Publishes an event synchronously to all current subscribers.</summary>
    void Publish<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;

    /// <summary>
    /// Subscribes to events of type <typeparamref name="TEvent"/>. Dispose the
    /// returned token to unsubscribe.
    /// </summary>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDomainEvent;
}
