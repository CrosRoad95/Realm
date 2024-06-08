using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Diagnostics.CodeAnalysis;
using ILogger = Serilog.ILogger;

namespace RealmCore.BlazorGui.Logging;

[ExcludeFromCodeCoverage]
public class RealmLogger
{
    private LoggerConfiguration _loggerConfiguration;
    private readonly LoggingLevelSwitch _levelSwitch = new();

    public LoggingLevelSwitch LevelSwitch => _levelSwitch;

    public LoggerConfiguration LoggerConfiguration => _loggerConfiguration;

    public RealmLogger(string appName, LogEventLevel logEventLevel = LogEventLevel.Information)
    {
        _levelSwitch.MinimumLevel = logEventLevel;
        _loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(_levelSwitch)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Components", LogEventLevel.Warning)
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                levelSwitch: _levelSwitch)
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

    public RealmLogger ByExcluding<T>()
    {
        _loggerConfiguration = _loggerConfiguration
            .Filter.ByExcluding(le => SourceContextEquals(le, typeof(T)));
        return this;
    }
    public RealmLogger ByExcluding(string sourceContext)
    {
        _loggerConfiguration = _loggerConfiguration
            .Filter.ByExcluding(le => SourceContextEquals(le, sourceContext));
        return this;
    }

    public RealmLogger ByExcluding(Func<LogEvent, bool> exclusionPredicate)
    {
        _loggerConfiguration = _loggerConfiguration.Filter.ByExcluding(exclusionPredicate);
        return this;
    }

    private static bool SourceContextEquals(LogEvent logEvent, Type sourceContext)
        => logEvent.Properties.GetValueOrDefault("SourceContext") is ScalarValue sv && sv.Value?.ToString() == sourceContext.FullName;

    private static bool SourceContextEquals(LogEvent logEvent, string sourceContext) => logEvent.Properties.GetValueOrDefault("SourceContext") is ScalarValue sv && sv.Value?.ToString() == sourceContext;

    public ILogger GetLogger() => _loggerConfiguration.CreateLogger();
}
