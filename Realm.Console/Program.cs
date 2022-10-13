using Realm.Console;
using Realm.Logging;
using Realm.MTARPGServer;

var serverConsole = new ServerConsole();
var logger = new Logger().GetLogger();
var configurationProvider = new Realm.Configuration.ConfigurationProvider();
var server = new MTARPGServerImpl(serverConsole, logger, configurationProvider);
var fileName = configurationProvider.Get<string>("General:ProvisioningFile");
try
{
    await server.BuildFromProvisioningFile(fileName);
    server.Start();
    serverConsole.Start();
}
catch(Exception ex)
{
    logger.Error(ex, "Failed to start server.");
}
