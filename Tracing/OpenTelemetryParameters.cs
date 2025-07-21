namespace Tracing;

public class OpenTelemetryParameters
{
    public string ActivitySourceName { get; init; } = "DefaultActivitySource";
    public string ServiceName { get; init; } = "DefaultService";
    public string ServiceVersion { get; init; } = "1.0.0";
    public bool Enabled { get; init; } = false; // ✅ Allows toggling OTel on/off

    public static OpenTelemetryParameters Default => new OpenTelemetryParameters();
}
