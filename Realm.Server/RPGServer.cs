namespace Realm.Server;

public partial class RPGServer : IReloadable, IRPGServer
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
    private readonly MtaServer<RPGPlayer> _server;
    private readonly Configuration _serverConfiguration;
    private readonly ScriptingConfiguration _scriptingConfiguration;
    private readonly ILogger _logger;

    public event Action<IRPGPlayer>? PlayerJoined;

    public RPGServer(IConfiguration configuration, ILogger logger, Action<ServerBuilder>? configureServerBuilder = null)
    {
        _logger = logger.ForContext<IRPGServer>();
        _serverConfiguration = configuration.GetSection("server").Get<Configuration>();
        _scriptingConfiguration = configuration.GetSection("scripting").Get<ScriptingConfiguration>();
        _server = MtaServer.CreateWithDiSupport<RPGPlayer>(
            builder =>
            {
                builder.ConfigureServer(configuration);
                if (configureServerBuilder != null)
                    configureServerBuilder(builder);

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(logger);
                    services.AddSingleton<IReloadable>(this);
                    services.AddSingleton<IRPGServer>(this);
                });
            }
        );
        var serverListConfiguration = configuration.GetSection("serverList").Get<ServerListConfiguration>();
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;
        _server.PlayerJoined += e => PlayerJoined?.Invoke(e);

        var startup = _server.GetRequiredService<Startup>();

        var _ = Task.Run(startup.StartAsync);

        Console.CancelKeyPress += (sender, args) =>
        {
            _server.Stop();
            _semaphore.Release();
        };
    }

    public void InitializeScripting(string fileName)
    {
        var code = File.ReadAllText(fileName);
        var scripting = _server.GetRequiredService<IScripting>();
        scripting.Execute(code, fileName);

        var typescriptDefinitions = scripting.GetTypescriptDefinition();
        var directory = Path.GetDirectoryName(fileName);
        File.WriteAllText(Path.Join(directory,"types.ts"), typescriptDefinitions);
    }

    private void StartScripting()
    {
        InitializeScripting("Server/startup.js");
    }

    public async Task Start()
    {
        if (_scriptingConfiguration.Enabled)
            StartScripting();

        _logger.Information("Server started at port: {port}", _serverConfiguration.Port);
        _server.Start();
        await _semaphore.WaitAsync();
    }

    public void Reload()
    {
        StartScripting();
    }

    public int GetPriority() => int.MaxValue;
}