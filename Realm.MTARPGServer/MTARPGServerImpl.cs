using System.IO;

namespace Realm.MTARPGServer;

public class MTARPGServerImpl
{
    private readonly RPGServer _rpgServer;
    private readonly Configuration.ConfigurationProvider _configurationProvider;
    private readonly string _basePath;

    public RPGServer Server => _rpgServer;

    public MTARPGServerImpl(IConsoleCommands consoleCommands, ILogger logger, Realm.Configuration.ConfigurationProvider configurationProvider, string basePath = "")
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(basePath);
        logger.Information("Starting server");
        _configurationProvider = configurationProvider;
        _basePath = basePath;
        _rpgServer = new RPGServer(_configurationProvider, logger, serverBuilder =>
        {
            serverBuilder.AddGuiFilesLocation("Gui");
            serverBuilder.AddLogic<TestLogic>();
            serverBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(consoleCommands);
                services.AddSingleton<Func<string>>(() => basePath);
            });
        });
        Directory.SetCurrentDirectory(previousDirectory);
    }

    public void Start()
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(_basePath);
        var serverTask = Task.Run(_rpgServer.Start);
        Directory.SetCurrentDirectory(previousDirectory);
    }
}