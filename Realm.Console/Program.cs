using Realm.Console;
using Realm.Logging;
using Realm.MTARPGServer;

var serverConsole = new ServerConsole();
var logger = new Logger().GetLogger();
var _configurationProvider = new Realm.Configuration.ConfigurationProvider();
var server = new MTARPGServerImpl(serverConsole, logger, _configurationProvider);
server.Start();
serverConsole.Start();