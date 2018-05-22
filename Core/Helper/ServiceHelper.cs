using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Core.Helper {
  public static class ServiceHelper {

    public static IServiceProvider BuildLoggerService() {
      return ServiceHelper.BuildLoggerService("");
    }

    public static IServiceProvider BuildLoggerService(string basePath) {
      ServiceCollection services = new ServiceCollection();

      services.AddTransient<LogHelper>();

      services.AddSingleton<ILoggerFactory, LoggerFactory>();
      services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
      services.AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Trace));

      ServiceProvider serviceProvider = services.BuildServiceProvider();

      ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      //configure NLog
      loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
      loggerFactory.ConfigureNLog(basePath + "nlog.config");

      return serviceProvider;
    }
  }
}
