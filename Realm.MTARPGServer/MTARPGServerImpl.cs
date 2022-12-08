using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Realm.MTARPGServer;

public class MTARPGServerImpl
{
    private readonly RPGServer _rpgServer;
    private readonly Configuration.RealmConfigurationProvider _configurationProvider;
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new Vector3Converter())
            .Build();

    public RPGServer Server => _rpgServer;
    public Configuration.RealmConfigurationProvider ConfigurationProvider => _configurationProvider;

    public MTARPGServerImpl(IConsoleCommands consoleCommands, ILogger logger, Realm.Configuration.RealmConfigurationProvider configurationProvider, IModule[] modules)
    {
        logger.Information("Starting server");
        _configurationProvider = configurationProvider;
        _rpgServer = new RPGServer(_configurationProvider, logger, modules, serverBuilder =>
        {
            serverBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(consoleCommands);
                services.AddSingleton<SeederServerBuilder>();
            });
        });
    }

    public async Task BuildFromSeedFiles(string[] seedFileNames)
    {
        var result = new JObject();
        var seedDatas = seedFileNames.Select(seedFileName => _deserializer.Deserialize<SeedData>(File.ReadAllText(seedFileName)));
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

        var seedValidator = new SeedValidator();
        await seedValidator.ValidateAndThrowAsync(seedData);
        var seedServerBuilder = _rpgServer.GetRequiredService<SeederServerBuilder>();
        await seedServerBuilder.BuildFrom(seedData);
    }

    public void Start()
    {
        var serverTask = Task.Run(_rpgServer.Start);
    }
}