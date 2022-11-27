using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Realm.MTARPGServer;

public class MTARPGServerImpl
{
    private readonly RPGServer _rpgServer;
    private readonly Configuration.RealmConfigurationProvider _configurationProvider;
    private readonly string? _basePath;
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new Vector3Converter())
            .Build();

    public RPGServer Server => _rpgServer;
    public Configuration.RealmConfigurationProvider ConfigurationProvider => _configurationProvider;

    public MTARPGServerImpl(IConsoleCommands consoleCommands, ILogger logger, Realm.Configuration.RealmConfigurationProvider configurationProvider, IModule[] modules, string? basePath = null)
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

    public async Task BuildFromSeedFiles(string[] seedFileNames)
    {
        var result = new JObject();
        var seedDatas = seedFileNames.Select(seedFileName => _deserializer.Deserialize<SeedData>(File.ReadAllText(Path.Join(_basePath, seedFileName))));
        foreach(var sourceObject in seedDatas)
        {
            var @object = JObject.Parse(JsonConvert.SerializeObject(sourceObject));
            result.Merge(@object, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });
        }
        var seedData = result.ToObject<SeedData>();
        if (seedData == null)
            throw new Exception("Failed to load see data.");

        var previousDirectory = Directory.GetCurrentDirectory();
        if (_basePath != null)
            Directory.SetCurrentDirectory(_basePath);

        var seedValidator = new SeedValidator();
        await seedValidator.ValidateAndThrowAsync(seedData);
        var seedServerBuilder = _rpgServer.GetRequiredService<SeederServerBuilder>();
        await seedServerBuilder.BuildFrom(seedData);
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