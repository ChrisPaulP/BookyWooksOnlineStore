using MassTransit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tracing;
using OpenTelemetry.Metrics;


public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetryTracing(this IServiceCollection services, IConfiguration configuration)
    {
        // ✅ Bind with safe defaults
        var openTelemetryParameters = configuration
            .GetSection("OpenTelemetry")
            .Get<OpenTelemetryParameters>() ?? OpenTelemetryParameters.Default;

        var jaegerSettings = configuration
            .GetSection("Jaeger")
            .Get<JaegerSettings>() ?? JaegerSettings.Default;

        // ✅ Skip entirely if disabled
        if (!openTelemetryParameters.Enabled)
        {
            Console.WriteLine("[DEBUG] OpenTelemetry tracing disabled via configuration.");
            return;
        }

        ActivitySourceProvider.Source =
            new System.Diagnostics.ActivitySource(openTelemetryParameters.ActivitySourceName);

        services.AddOpenTelemetry().WithTracing(tracing =>
        {
            tracing.AddSource(openTelemetryParameters.ActivitySourceName)
                   .AddSource(DiagnosticHeaders.DefaultListenerName)
                   .ConfigureResource(resource =>
                   {
                       resource.AddService(openTelemetryParameters.ServiceName,
                           serviceVersion: openTelemetryParameters.ServiceVersion);
                   });

            tracing.AddAspNetCoreInstrumentation(o =>
            {
                o.Filter = context =>
                {
                    var path = context.Request.Path.Value;
                    return !string.IsNullOrEmpty(path) &&
                           (path.Contains("Api", StringComparison.InvariantCulture) ||
                            path.Contains("Orders", StringComparison.InvariantCulture));
                };

                o.EnrichWithHttpRequest = (activity, httpRequest) =>
                    activity.SetTag("requestProtocol", httpRequest.Protocol);

                o.EnrichWithHttpResponse = (activity, httpResponse) =>
                    activity.SetTag("responseLength", httpResponse.ContentLength);

                o.RecordException = true;
                o.EnrichWithException = (activity, exception) =>
                {
                    activity.SetTag("exceptionType", exception.GetType().ToString());
                    activity.SetTag("stackTrace", exception.StackTrace);
                };
            });

            tracing.AddHttpClientInstrumentation();
            tracing.AddEntityFrameworkCoreInstrumentation(opt =>
            {
                opt.SetDbStatementForText = true;
                opt.SetDbStatementForStoredProcedure = true;
                opt.EnrichWithIDbCommand = (activity, command) =>
                {
                    var stateDisplayName = $"{command.CommandType} main";
                    activity.DisplayName = stateDisplayName;
                    activity.SetTag("db.name", stateDisplayName);
                };
            });

            // ✅ Always safe because of defaults
            tracing.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri($"{jaegerSettings.Protocol}://{jaegerSettings.Host}:{jaegerSettings.Port}");
            });
        });
    }

    public static void AddOpenTelemetryMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        var openTelemetryParameters = configuration
            .GetSection("OpenTelemetry")
            .Get<OpenTelemetryParameters>() ?? OpenTelemetryParameters.Default;

        if (!openTelemetryParameters.Enabled)
        {
            Console.WriteLine("[DEBUG] OpenTelemetry metrics disabled via configuration.");
            return;
        }

        services.AddOpenTelemetry().WithMetrics(options =>
        {
            options.AddMeter(openTelemetryParameters.ServiceName);
            options.AddPrometheusExporter();
            options.ConfigureResource(resource =>
            {
                resource.AddService(serviceName: openTelemetryParameters.ServiceName,
                    serviceVersion: openTelemetryParameters.ServiceVersion);
            });
        });
    }
}

