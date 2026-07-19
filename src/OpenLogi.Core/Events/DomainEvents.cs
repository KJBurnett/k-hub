using OpenLogi.Core.Devices;

namespace OpenLogi.Core.Events;

/// <summary>Marker interface for an in-process domain event.</summary>
public interface IDomainEvent
{
    /// <summary>When the event occurred (UTC).</summary>
    DateTimeOffset TimestampUtc { get; }
}

/// <summary>Base record capturing the timestamp shared by all domain events.</summary>
public abstract record DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>Raised when a device is connected / enumerated (PLAN.md section 16).</summary>
public sealed record DeviceConnectedEvent(DeviceInfo Device) : DomainEvent;

/// <summary>Raised when a device is disconnected.</summary>
public sealed record DeviceDisconnectedEvent(DeviceIdentity Identity) : DomainEvent;

/// <summary>Raised when the active profile changes.</summary>
public sealed record ProfileSwitchedEvent(string DeviceKey, string ProfileId) : DomainEvent;

/// <summary>Raised when a macro finishes executing.</summary>
public sealed record MacroExecutedEvent(string MacroId) : DomainEvent;

/// <summary>Raised when an unrecognised HID packet is observed (PLAN.md section 18).</summary>
public sealed record UnknownHidPacketEvent(string DeviceKey, byte[] Payload) : DomainEvent;
