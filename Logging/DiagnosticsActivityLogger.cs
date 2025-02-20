namespace Logging;

public class DiagnosticsActivityLogger : IDiagnosticsActivityLogger
{
    private readonly ILogger<DiagnosticsActivityLogger> _logger;
    public DiagnosticsActivityLogger(ILogger<DiagnosticsActivityLogger> logger)
    {
        _logger = logger;  
    }
    public void LogStart(ApplicationName applicationName, Event eventName, string message)
    {
        var activity = Activity.Current ?? ActivitySourceProvider.Source.StartActivity();
        activity?.AddEvent(new(message));
        OpenTelemetryMetricConfiguration.IncrementEventCounter(applicationName, eventName).IfFail(ex => _logger.LogInformation($"The Increment Counter has failed to work because of the following: {ex.Message}")); ;
    }

    public void LogSuccess(string message)
    {
        Activity.Current?.AddEvent(new(message));
    }

    public void LogError(string message)
    {
        Activity.Current?.AddEvent(new(message));
    }

}
