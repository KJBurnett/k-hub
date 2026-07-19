using OpenLogi.Core.Devices;
using OpenLogi.Core.Events;

namespace OpenLogi.Core.Tests;

public class EventBusTests
{
    [Fact]
    public void Publish_invokes_subscribers()
    {
        var bus = new EventBus();
        var received = 0;
        bus.Subscribe<MacroExecutedEvent>(_ => received++);

        bus.Publish(new MacroExecutedEvent("m1"));

        Assert.Equal(1, received);
    }

    [Fact]
    public void Disposing_subscription_stops_delivery()
    {
        var bus = new EventBus();
        var received = 0;
        var subscription = bus.Subscribe<MacroExecutedEvent>(_ => received++);

        subscription.Dispose();
        bus.Publish(new MacroExecutedEvent("m1"));

        Assert.Equal(0, received);
    }

    [Fact]
    public void Handlers_of_other_types_are_not_invoked()
    {
        var bus = new EventBus();
        var received = 0;
        bus.Subscribe<MacroExecutedEvent>(_ => received++);

        bus.Publish(new DeviceDisconnectedEvent(new DeviceIdentity(0x046D, 0xC08B)));

        Assert.Equal(0, received);
    }

    [Fact]
    public void Failing_handler_does_not_prevent_others_and_is_reported()
    {
        var bus = new EventBus();
        var reached = false;
        bus.Subscribe<MacroExecutedEvent>(_ => throw new InvalidOperationException("boom"));
        bus.Subscribe<MacroExecutedEvent>(_ => reached = true);

        Assert.Throws<AggregateException>(() => bus.Publish(new MacroExecutedEvent("m1")));
        Assert.True(reached);
    }
}
