using Realm.Interfaces.Discord;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Realm.Logging;

public class Logger
{
    private LoggerConfiguration _loggerConfiguration;
    public Logger()
    {
        _loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Filter.ByExcluding(le => SourceContextEquals(le, typeof(IDiscord)))
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug);
    }

    public Logger WithSink(ILogEventSink sink)
    {
        _loggerConfiguration = _loggerConfiguration.WriteTo.Sink(sink);
        return this;
    }

    private static bool SourceContextEquals(LogEvent logEvent, Type sourceContext) => logEvent.Properties.GetValueOrDefault("SourceContext") is ScalarValue sv && sv.Value?.ToString() == sourceContext.FullName;

    public ILogger GetLogger() => _loggerConfiguration.CreateLogger();
}
