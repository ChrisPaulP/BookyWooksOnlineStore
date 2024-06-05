using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tracing;


public static class OpenTelemetryMetricConfiguration
{
    // Catalogue metrics
    private static readonly Meter CatalogueMeter = new("Bookywooks.Catalogue.Api");
    public static readonly Counter<int> OrderConsumedEventCounter = CatalogueMeter.CreateCounter<int>("user.created.event.count");

    // Order metrics
    private static readonly Meter OrderMeter = new("Bookwooks.Orderapi.Web");
    public static readonly Counter<int> OrderStartedEventCounter = OrderMeter.CreateCounter<int>("order.started.event.count");
    public static readonly Counter<int> OrderLongRunningRequestCounter = OrderMeter.CreateCounter<int>("order.long.running.request.count");
    public static readonly Histogram<long> OrderMethodDuration = OrderMeter.CreateHistogram<long>("order.method.duration", "milliseconds");

}
