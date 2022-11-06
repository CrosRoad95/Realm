namespace Realm.MTARPGServer;

public class MTARPGServerImpl
{
    private readonly RPGServer _rpgServer;
    private readonly Configuration.ConfigurationProvider _configurationProvider;
    private readonly string? _basePath;

    public RPGServer Server => _rpgServer;
    public Configuration.ConfigurationProvider ConfigurationProvider => _configurationProvider;

    public MTARPGServerImpl(IConsoleCommands consoleCommands, ILogger logger, Realm.Configuration.ConfigurationProvider configurationProvider, IModule[] modules, string? basePath = null)
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        if(basePath != null)
            Directory.SetCurrentDirectory(basePath);
        logger.Information("Starting server");
        _configurationProvider = configurationProvider;
        _basePath = basePath;
        _rpgServer = new RPGServer(_configurationProvider, logger, modules, serverBuilder =>
        {
            serverBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(consoleCommands);
                services.AddSingleton<SeederServerBuilder>();
                services.AddSingleton<Func<string?>>(() => basePath);
            });
        }, basePath);

        Directory.SetCurrentDirectory(previousDirectory);
    }

    public async Task BuildFromSeedFile(string seedFileName)
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        var seedSource = await File.ReadAllTextAsync(Path.Join(_basePath, seedFileName));
        if (_basePath != null)
            Directory.SetCurrentDirectory(_basePath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new Vector3Converter())
            .Build();

        var seed = deserializer.Deserialize<Seed>(seedSource);
        var seedValidator = new SeedValidator();
        await seedValidator.ValidateAndThrowAsync(seed);
        var seedServerBuilder = _rpgServer.GetRequiredService<SeederServerBuilder>();
        await seedServerBuilder.BuildFrom(seed);
        Directory.SetCurrentDirectory(previousDirectory);
    }

    public void Start()
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        if(_basePath != null)
            Directory.SetCurrentDirectory(_basePath);
        var serverTask = Task.Run(_rpgServer.Start);
        Directory.SetCurrentDirectory(previousDirectory);
    }
}