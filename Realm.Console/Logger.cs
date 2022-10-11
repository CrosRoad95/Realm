using Realm.Interfaces.Discord;
using Serilog;
using Serilog.Events;

namespace Realm.Console;

public class Logger
{
    private readonly ILogger _logger;
    public Logger()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Filter.ByExcluding(le => SourceContextEquals(le, typeof(IDiscord)))
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
            .CreateLogger();
    }

    private static bool SourceContextEquals(LogEvent logEvent, Type sourceContext) => logEvent.Properties.GetValueOrDefault("SourceContext") is ScalarValue sv && sv.Value?.ToString() == sourceContext.FullName;


    public ILogger GetLogger() => _logger;
}
