using Realm.Console;
using Realm.Discord;
using Realm.Interfaces.Discord;
using Realm.Interfaces.Extend;
using Realm.Logging;
using Realm.MTARPGServer;
using Realm.Persistance;
using Realm.Scripting;
using Realm.Server;

var serverConsole = new ServerConsole();
var logger = new Logger()
    .ByExcluding<IDiscord>()
    .GetLogger();
var configurationProvider = new Realm.Configuration.ConfigurationProvider();
var server = new MTARPGServerImpl(serverConsole, logger, configurationProvider, new IModule[]
        {
            new DiscordModule(),
            new IdentityModule(),
            new ScriptingModule(),
            new ServerScriptingModule(),
        });
var fileName = configurationProvider.Get<string>("General:ProvisioningFile");
try
{
    await server.BuildFromProvisioningFile(fileName);
    server.Start();
    serverConsole.Start();
}
catch (Exception ex)
{
    logger.Error(ex, "Failed to start server.");
}
