namespace Realm.MTARPGServer;

public class MTARPGServerImpl
{
    private readonly RPGServer _rpgServer;
    private readonly Configuration.ConfigurationProvider _configurationProvider;

    private RPGServer Server => _rpgServer;

    public MTARPGServerImpl(IConsoleCommands consoleCommands, ILogger logger, Realm.Configuration.ConfigurationProvider configurationProvider)
    {
        logger.Information("Starting server");
        _configurationProvider = configurationProvider;

        _rpgServer = new RPGServer(_configurationProvider, logger, serverBuilder =>
        {
            serverBuilder.AddGuiFilesLocation("Gui");
            serverBuilder.AddLogic<TestLogic>();
            serverBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(consoleCommands);
            });
        });
        _configurationProvider = configurationProvider;
    }

    public void Start()
    {
        var serverTask = Task.Run(_rpgServer.Start);
    }
}