﻿namespace Tracing;

public class LogContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogContextMiddleware> _logger;

    public LogContextMiddleware(RequestDelegate next, ILogger<LogContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public Task InvokeAsync(HttpContext context)
    {
        // var correlationHeaders = Activity.Current?.Baggage.ToDictionary(b => b.Key, b => (object)b.Value);
        var traceId = Activity.Current?.TraceId.ToString();

        using (_logger.BeginScope("{@TraceId}", traceId))
        {
            return _next(context);
        }
    }
}
