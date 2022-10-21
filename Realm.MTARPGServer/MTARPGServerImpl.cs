using Realm.Discord;
using Realm.Interfaces.Extend;
using Realm.Scripting;
using Realm.Server.Scripting;

namespace Realm.MTARPGServer;

public class Vector3Converter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(Vector3);

    public object? ReadYaml(IParser parser, Type type)
    {
        parser.Consume<SequenceStart>();
        var x = float.Parse(parser.Consume<Scalar>().Value);
        var y = float.Parse(parser.Consume<Scalar>().Value);
        var z = float.Parse(parser.Consume<Scalar>().Value);
        parser.Consume<SequenceEnd>();
        return new Vector3(x,y,z);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        throw new NotImplementedException();
    }
}

public class MTARPGServerImpl
{
    private readonly RPGServer _rpgServer;
    private readonly Configuration.ConfigurationProvider _configurationProvider;
    private readonly string? _basePath;

    public RPGServer Server => _rpgServer;

    public MTARPGServerImpl(IConsoleCommands consoleCommands, ILogger logger, Realm.Configuration.ConfigurationProvider configurationProvider, string? basePath = null)
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        if(basePath != null)
            Directory.SetCurrentDirectory(basePath);
        logger.Information("Starting server");
        _configurationProvider = configurationProvider;
        _basePath = basePath;
        _rpgServer = new RPGServer(_configurationProvider, logger, new IModule[]
        {
            new DiscordModule(),
            new ScriptingModule(),
            new ServerScriptingModule(),
        }, serverBuilder =>
        {
            serverBuilder.AddGuiFilesLocation("Gui");
            serverBuilder.ConfigureServices(services =>
            {
                services.AddSingleton(consoleCommands);
                services.AddSingleton<Func<string?>>(() => basePath);
            });
        });
        Directory.SetCurrentDirectory(previousDirectory);
    }

    public async Task BuildFromProvisioningFile(string provisioningFileName)
    {
        var previousDirectory = Directory.GetCurrentDirectory();
        var provisioningSource = await File.ReadAllTextAsync(Path.Join(_basePath, provisioningFileName));
        if (_basePath != null)
            Directory.SetCurrentDirectory(_basePath);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new Vector3Converter())
            .Build();

        var provisioning = deserializer.Deserialize<Provisioning>(provisioningSource);
        var provisioningValidator = new ProvisioningValidator();
        await provisioningValidator.ValidateAndThrowAsync(provisioning);
        using (var _ = new PersistantScope())
        {
            var elementFunctions = _rpgServer.GetRequiredService<ElementFunctions>();
            foreach (var pair in provisioning.Spawns)
            {
                elementFunctions.CreateSpawn(pair.Key, pair.Value.Name, pair.Value.Position, pair.Value.Rotation);
            }
        }
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