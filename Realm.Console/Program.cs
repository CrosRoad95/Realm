using Realm.Console;
using Realm.Discord;
using Realm.GRpc;
using Realm.Interfaces.Extend;
using Realm.Logging;
using Realm.Scripting;
using Realm.Server;
using Realm.Server.Modules;
using Serilog;
using Serilog.Events;

var serverConsole = new ServerConsole();
var serilogLogger = new RealmLogger(LogEventLevel.Verbose)
    .AddSeq();

var logger = serilogLogger.GetLogger();
var configurationProvider = new Realm.Configuration.RealmConfigurationProvider();
var server = new MTARPGServerImpl(serverConsole, logger, configurationProvider, new IModule[]
    {
        new DiscordModule(),
        new IdentityModule(),
        new ScriptingModule(),
        new ServerScriptingModule(),
        new GrpcModule(),
    });
try
{
    await server.BuildFromSeedFiles();
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