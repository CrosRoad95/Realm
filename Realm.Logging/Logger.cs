using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Realm.Logging;

public class Logger
{
    private LoggerConfiguration _loggerConfiguration;
    public Logger(LogEventLevel logEventLevel = LogEventLevel.Debug)
    {
        _loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(logEventLevel)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {this}{padding}{Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: logEventLevel)
            .Enrich.FromLogContext();
    }

    public Logger WithSink(ILogEventSink sink)
    {
        _loggerConfiguration = _loggerConfiguration.WriteTo.Sink(sink);
        return this;
    }

    public Logger ByExcluding<T>()
    {
        _loggerConfiguration = _loggerConfiguration
            .Filter.ByExcluding(le => SourceContextEquals(le, typeof(T)));
        return this;
    }

    private static bool SourceContextEquals(LogEvent logEvent, Type sourceContext) => logEvent.Properties.GetValueOrDefault("SourceContext") is ScalarValue sv && sv.Value?.ToString() == sourceContext.FullName;

    public ILogger GetLogger() => _loggerConfiguration.CreateLogger();
}
