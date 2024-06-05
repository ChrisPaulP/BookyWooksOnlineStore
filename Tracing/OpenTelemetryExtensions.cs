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
        services.Configure<OpenTelemetryParameters>(configuration.GetSection("OpenTelemetry"));
        var openTelemetryParameters = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryParameters>();
        var y = configuration.GetSection("OpenTelemetry");

        var jaegerSettings = configuration.GetSection("Jaeger").Get<JaegerSettings>();

        ActivitySourceProvider.Source = new System.Diagnostics.ActivitySource(openTelemetryParameters.ActivitySourceName);

        services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
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
                // to trace only api requests
                //o.Filter = (context) => !string.IsNullOrEmpty(context.Request.Path.Value) && context.Request.Path.Value.Contains("Api", StringComparison.InvariantCulture);
                //o.Filter = (context) =>
                //{
                //    var path = context.Request.Path.Value;
                //    return !string.IsNullOrEmpty(path) && path.Contains("Orders", StringComparison.InvariantCulture);
                //};
                // example: only collect telemetry about HTTP GET requests
                // return httpContext.Request.Method.Equals("GET");

                // enrich activity with http request and response
                o.EnrichWithHttpRequest = (activity, httpRequest) => { activity.SetTag("requestProtocol", httpRequest.Protocol); };
                o.EnrichWithHttpResponse = (activity, httpResponse) => { activity.SetTag("responseLength", httpResponse.ContentLength); };

                // automatically sets Activity Status to Error if an unhandled exception is thrown
                o.RecordException = true;
                o.EnrichWithException = (activity, exception) =>
                {
                    activity.SetTag("exceptionType", exception.GetType().ToString());
                    activity.SetTag("stackTrace", exception.StackTrace);
                };
            });
            tracing.AddHttpClientInstrumentation(); // This enables HttpClient instrumentation for outgoing requests

            tracing.AddEntityFrameworkCoreInstrumentation(opt =>
            {
                opt.SetDbStatementForText = true;
                opt.SetDbStatementForStoredProcedure = true;
                opt.EnrichWithIDbCommand = (activity, command) =>
                {
                    // var stateDisplayName = $"{command.CommandType} main";
                    // activity.DisplayName = stateDisplayName;
                    // activity.SetTag("db.name", stateDisplayName);
                };
            });

            //options.AddConsoleExporter();
            //tracing.AddOtlpExporter();
            tracing.AddOtlpExporter(options =>
            {
                options.Endpoint =
                            new Uri(
                                $"{jaegerSettings.Protocol}://{jaegerSettings.Host}:{jaegerSettings.Port}");
            });  // is used to display data on Jaeger.

        });
    }

    public static void AddOpenTelemetryMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        var x = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryParameters>();
        services.Configure<OpenTelemetryParameters>(configuration.GetSection("OpenTelemetry"));
        var openTelemetryParameters = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryParameters>();

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
