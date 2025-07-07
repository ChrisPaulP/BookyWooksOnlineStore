using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz.Logging;
using Logger = Quartz.Logging.Logger;
using LogLevel = Quartz.Logging.LogLevel;

namespace BookWooks.OrderApi.Infrastructure.Quartz;
internal class SerilogLogProvider : ILogProvider
{
  private readonly ILogger _logger;

  internal SerilogLogProvider(ILogger logger)
  {
    _logger = logger;
  }

  public Logger GetLogger(string name)
  {
    return (level, func, exception, parameters) =>
    {
      if (func == null)
      {
        return true;
      }

      if (level == LogLevel.Debug || level == LogLevel.Trace)
      {
        _logger.LogDebug(exception, func(), parameters);
      }

      if (level == LogLevel.Info)
      {
        _logger.LogInformation(exception, func(), parameters);
      }

      if (level == LogLevel.Warn)
      {
        _logger.LogWarning(exception, func(), parameters);
      }

      if (level == LogLevel.Error)
      {
        _logger.LogError(exception, func(), parameters);
      }

      if (level == LogLevel.Fatal)
      {
        _logger.LogCritical(exception, func(), parameters);
      }

      return true;
    };
  }
  public IDisposable OpenNestedContext(string message)
  {
    throw new NotImplementedException();
  }

  public IDisposable OpenMappedContext(string key, string value)
  {
    throw new NotImplementedException();
  }

  public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
  {
    throw new NotImplementedException();
  }
}
