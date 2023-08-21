using RealmCore.Configuration;
using RealmCore.Logging;
using Serilog.Events;

namespace RealmCore.Console.Utilities;

public static class RPGServerBuilderExtensions
{
    public static RPGServerBuilder AddDefaultConsole(this RPGServerBuilder serverBuilder)
    {
        serverBuilder.AddConsole(typeof(DefaultServerConsole));
        return serverBuilder;
    }

    public static RPGServerBuilder AddDefaultLogger(this RPGServerBuilder serverBuilder, string? appName = null, string? seqServerUrl = null)
    {
        var realmLogger = new RealmLogger(appName ?? "RealmCore", LogEventLevel.Information);

        serverBuilder.AddLogger(realmLogger.GetLogger());
        return serverBuilder;
    }

    public static RPGServerBuilder AddDefaultConfiguration(this RPGServerBuilder serverBuilder)
    {
        var configurationProvider = new RealmConfigurationProvider();
        serverBuilder.AddConfiguration(configurationProvider);
        return serverBuilder;
    }
}
