using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Realm.Logging;

public class Logger
{
    private LoggerConfiguration _loggerConfiguration;
    private LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch();

    public LoggingLevelSwitch LevelSwitch => levelSwitch;
    public Logger(LogEventLevel logEventLevel = LogEventLevel.Debug)
    {
        levelSwitch.MinimumLevel = logEventLevel;
        _loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {userFriendlyName}{padding}{Message:lj}{NewLine}{Exception}",
                levelSwitch: levelSwitch)
            .Enrich.FromLogContext();
    }

    public Logger WithSink(ILogEventSink sink)
    {
        _loggerConfiguration = _loggerConfiguration.WriteTo.Sink(sink);
        return this;
    }

    public LoggerSinkConfiguration GetSinkConfiguration() => _loggerConfiguration.WriteTo;

    public Logger ByExcluding<T>()
    {
        _loggerConfiguration = _loggerConfiguration
            .Filter.ByExcluding(le => SourceContextEquals(le, typeof(T)));
        return this;
    }

    private static bool SourceContextEquals(LogEvent logEvent, Type sourceContext) => logEvent.Properties.GetValueOrDefault("SourceContext") is ScalarValue sv && sv.Value?.ToString() == sourceContext.FullName;

    public ILogger GetLogger() => _loggerConfiguration.CreateLogger();
}
