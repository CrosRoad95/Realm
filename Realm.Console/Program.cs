RPGServer? server = null;
var serverConsole = new ServerConsole();
var logger = new Logger().GetLogger();
var configuration = new Realm.Configuration.ConfigurationProvider();
logger.Information("Starting server");

try
{
    server = new RPGServer(configuration, logger, serverBuilder =>
    {
        serverBuilder.AddGuiFilesLocation("Gui");
        serverBuilder.AddLogic<TestLogic>();
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IConsoleCommands>(serverConsole);
        });
    });
    var serverTask = Task.Run(server.Start);
    if(configuration.Get<bool>("DOTNET_RUNNING_IN_CONTAINER") == true)
    {
        serverTask.Wait();
    }
    else
        serverConsole.Start();
}
catch (Exception exception)
{
    logger.Error(exception, "Error in startup");
    Thread.Sleep(2000);
}