using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Net.Sockets;
using System.Xml.Linq;

namespace Realm.Logging;

public class RealmLogger
{
    private LoggerConfiguration _loggerConfiguration;
    private LoggingLevelSwitch levelSwitch = new LoggingLevelSwitch();

    public LoggingLevelSwitch LevelSwitch => levelSwitch;
    public RealmLogger(string appName, LogEventLevel logEventLevel = LogEventLevel.Debug)
    {
        levelSwitch.MinimumLevel = logEventLevel;
        _loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(levelSwitch)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                levelSwitch: levelSwitch)
            .Enrich.WithProperty("AppName", appName)
            .Enrich.WithThreadId()
            .Enrich.With<ActivityEnricher>()
            .Enrich.FromLogContext();
    }

    public RealmLogger WithSink(ILogEventSink sink)
    {
        _loggerConfiguration = _loggerConfiguration.WriteTo.Sink(sink);
        return this;
    }

    public RealmLogger AddSeq(string serverUrl)
    {
        _loggerConfiguration.WriteTo.Seq(serverUrl, controlLevelSwitch: LevelSwitch);
        return this;
    }

    public RealmLogger ByExcluding<T>()
    {
        _loggerConfiguration = _loggerConfiguration
            .Filter.ByExcluding(le => SourceContextEquals(le, typeof(T)));
        return this;
    }

    private static bool SourceContextEquals(LogEvent logEvent, Type sourceContext) => logEvent.Properties.GetValueOrDefault("SourceContext") is ScalarValue sv && sv.Value?.ToString() == sourceContext.FullName;

    public ILogger GetLogger() => _loggerConfiguration.CreateLogger();
}
