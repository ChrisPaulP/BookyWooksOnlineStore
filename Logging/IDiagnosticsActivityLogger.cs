namespace Logging;
public interface IDiagnosticsActivityLogger
{
    void LogStart(ApplicationName applicationName, Event eventName, string message);
    void LogSuccess(string message);
    void LogError(string message);
}
