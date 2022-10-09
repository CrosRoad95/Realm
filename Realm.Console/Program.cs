DefaultMtaServer? server = null;
var serverConsole = new ServerConsole();
var logger = new Logger().GetLogger();

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false)
    .AddJsonFile("appsettings.development.json", true, true)
    .AddJsonFile("appsettings.local.json", true, true)
    .AddEnvironmentVariables()
    .Build();

logger.Information("Starting server");

try
{
    server = new DefaultMtaServer(configuration, logger, serverBuilder =>
    {
        serverBuilder.AddGuiFilesLocation("Gui");
        serverBuilder.AddLogic<TestLogic>();
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IConsoleCommands>(serverConsole);
        });
    });
    var serverTask = Task.Run(server.Start);
    if(configuration.GetSection("DOTNET_RUNNING_IN_CONTAINER").Value == "true")
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