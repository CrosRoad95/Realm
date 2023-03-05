using Realm.Configuration;
using Realm.Logging;
using Realm.Server;
using Serilog.Events;

namespace Realm.ConsoleUtilities;

public static class RPGServerBuilderExtensions
{
    public static RPGServerBuilder AddDefaultConsole(this RPGServerBuilder serverBuilder)
    {
        serverBuilder.AddConsole(new DefaultServerConsole());
        return serverBuilder;
    }

    public static RPGServerBuilder AddDefaultLogger(this RPGServerBuilder serverBuilder, string? appName = null, string? seqServerUrl = null)
    {
        var logger = new RealmLogger(appName ?? "RPGServer", LogEventLevel.Verbose)
            .AddSeq(seqServerUrl ?? "http://localhost:5341")
            .GetLogger();
        serverBuilder.AddLogger(logger);
        return serverBuilder;
    }

    public static RPGServerBuilder AddDefaultConfiguration(this RPGServerBuilder serverBuilder)
    {
        var configurationProvider = new RealmConfigurationProvider();
        serverBuilder.AddConfiguration(configurationProvider);
        return serverBuilder;
    }
}
