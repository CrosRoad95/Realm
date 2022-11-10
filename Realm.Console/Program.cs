using Realm.Console;
using Realm.Discord;
using Realm.Interfaces.Discord;
using Realm.Interfaces.Extend;
using Realm.Logging;
using Realm.MTARPGServer;
using Realm.Persistance;
using Realm.Scripting;
using Realm.Server;
using Serilog;
using Serilog.Events;

var serverConsole = new ServerConsole();
var serilogLogger = new Logger(LogEventLevel.Verbose)
    .ByExcluding<IDiscord>();

serilogLogger.GetSinkConfiguration().Seq("http://localhost:5341", controlLevelSwitch: serilogLogger.LevelSwitch);
var logger = serilogLogger.GetLogger();
var configurationProvider = new Realm.Configuration.ConfigurationProvider();
var server = new MTARPGServerImpl(serverConsole, logger, configurationProvider, new IModule[]
        {
            new DiscordModule(),
            new IdentityModule(),
            new ScriptingModule(),
            new ServerScriptingModule(),
        });
var seedFiles = configurationProvider.Get<string[]>("General:SeedFiles");
try
{
    await server.BuildFromSeedFiles(seedFiles);
    server.Start();
    serverConsole.Start();
}
catch (Exception ex)
{
    logger.Error(ex, "Failed to start server.");
}
finally
{
    await Log.CloseAndFlushAsync();
}