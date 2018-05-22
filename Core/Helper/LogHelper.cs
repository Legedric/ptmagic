using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Core.Helper {
  public class LogHelper {
    private readonly ILogger<LogHelper> log;

    public LogHelper(ILogger<LogHelper> logger) {
      log = logger;
    }

    public void DoLogInfo(string message) {
      if (log.IsEnabled(LogLevel.Information)) log.LogInformation(message);
    }

    public void DoLogWarn(string message) {
      if (log.IsEnabled(LogLevel.Warning)) log.LogWarning(message);
    }

    public void DoLogError(string message) {
      if (log.IsEnabled(LogLevel.Error)) log.LogError(message);
    }

    public void DoLogCritical(string message, System.Exception ex) {
      if (log.IsEnabled(LogLevel.Critical)) log.LogCritical(ex, message);
    }

    public void DoLogDebug(string message) {
      if (log.IsEnabled(LogLevel.Debug)) log.LogDebug(message);
    }
  }
}
