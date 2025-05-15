namespace Tracing;
public static class OpenTelemetryMetricConfiguration
{
    // Meters
    private static readonly Meter CatalogueMeter = new("Bookywooks.Catalogue.Api");
    private static readonly Meter OrderMeter = new("Bookwooks.Orderapi.Web");
    private static readonly Meter IdentityMeter = new("BookyWooks.Identity");

    // Metric Counters (Immutable Map from LanguageExt)
    private static readonly Map<ApplicationName, Map<Event, Counter<int>>> MetricsMap = Map(
        (ApplicationName.Catalogue, Map(
            (Event.OrderConsumed, CatalogueMeter.CreateCounter<int>("order.consumed.event.count"))
        )),
        (ApplicationName.Order, Map(
            (Event.OrderStarted, OrderMeter.CreateCounter<int>("order.started.event.count")),
            (Event.OrderConsumed, OrderMeter.CreateCounter<int>("order.consumed.event.count")),
            (Event.OrderLongRunningRequest, OrderMeter.CreateCounter<int>("order.long.running.request.count"))
        )),
        (ApplicationName.Identity, Map(
            (Event.UserCreated, IdentityMeter.CreateCounter<int>("user.created.event.count")),
            (Event.UserDeleted, IdentityMeter.CreateCounter<int>("user.deleted.event.count")),
            (Event.UserUpdated, IdentityMeter.CreateCounter<int>("user.updated.event.count")),
            (Event.UserLoggedIn, IdentityMeter.CreateCounter<int>("user.loggedin.event.count")),
            (Event.UserLoggedOut, IdentityMeter.CreateCounter<int>("user.loggedout.event.count"))
        ))
    );

    // Increment Metric Counter
    public static Try<Unit> IncrementEventCounter(ApplicationName applicationName, Event eventName) =>
        Try(() =>
            MetricsMap
                .Find(applicationName)              
                .Bind(metrics => metrics.Find(eventName))
                .Match(
                    Some: counter => { counter.Add(1, new KeyValuePair<string, object?>("event.name", eventName)); return unit; }, 
                    None: () => throw new ArgumentException($"Invalid category '{applicationName}' or event '{eventName}'") 
                )
        );
}


