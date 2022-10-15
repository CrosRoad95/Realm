namespace Realm.Server;

public partial class RPGServer : IReloadable, IRPGServer
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
    private readonly MtaServer<RPGPlayer> _server;
    private readonly SlipeServerConfiguration _serverConfiguration;
    private readonly ScriptingConfiguration _scriptingConfiguration;
    private readonly ILogger _logger;
    private readonly LuaEventContextFactory _luaEventContextFactory;

    public event Action<IRPGPlayer>? PlayerJoined;

    private readonly List<(string, Func<ILuaEventContext, Task>)> _eventsSubscribers = new();

    public RPGServer(ConfigurationProvider configurationProvider, ILogger logger, Action<ServerBuilder>? configureServerBuilder = null)
    {
        _logger = logger.ForContext<IRPGServer>();
        _serverConfiguration = configurationProvider.Get<SlipeServerConfiguration>("server");
        _scriptingConfiguration = configurationProvider.Get<ScriptingConfiguration>("scripting");
        _server = MtaServer.CreateWithDiSupport<RPGPlayer>(
            builder =>
            {
                builder.ConfigureServer(configurationProvider.Configuration);
                if (configureServerBuilder != null)
                    configureServerBuilder(builder);

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(configurationProvider);
                    services.AddSingleton(logger);
                    services.AddSingleton<IReloadable>(this);
                    services.AddSingleton<IRPGServer>(this);
                    services.AddSingleton<LuaEventContextFactory>();
                });
            }
        );
        var serverListConfiguration = configurationProvider.Get<ServerListConfiguration>("serverList");
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;
        _server.PlayerJoined += e => PlayerJoined?.Invoke(e);
        _server.LuaEventTriggered += Server_LuaEventTriggered;

        var startup = _server.GetRequiredService<Startup>();
        _luaEventContextFactory = _server.GetRequiredService<LuaEventContextFactory>();

        var _ = Task.Run(startup.StartAsync);

        Console.CancelKeyPress += (sender, args) =>
        {
            _server.Stop();
            _semaphore.Release();
        };
    }

    private async void Server_LuaEventTriggered(LuaEvent luaEvent)
    {
        var context = _luaEventContextFactory.CreateContextFromLuaEvent(luaEvent);
        foreach (var pair in _eventsSubscribers)
        {
            if(pair.Item1 == luaEvent.Name)
            {
                await pair.Item2(context);
            }
        }
    }

    public void SubscribeLuaEvent(string eventName, Func<ILuaEventContext, Task> callback)
    {
        _eventsSubscribers.Add((eventName, callback));
    }

    public TService GetRequiredService<TService>() where TService: notnull
    {
        return _server.GetRequiredService<TService>();
    }

    public void InitializeScripting(string fileName)
    {
        _logger.Information("Initializing startup.js: {fileName}", fileName);
        try
        {
            var code = File.ReadAllText(fileName);
            var scripting = _server.GetRequiredService<IScripting>();
            scripting.Execute(code, fileName);

            var typescriptDefinitions = scripting.GetTypescriptDefinition();
            var directory = Path.GetDirectoryName(fileName);
            File.WriteAllText(Path.Join(directory,"types.ts"), typescriptDefinitions);
            _logger.Information("Scripting initialized, created types.js");
        }
        catch(Exception ex)
        {
            _logger.Error(ex, "Failed to initialize scripting!");
        }
    }

    private void StartScripting()
    {
        var path = _server.GetRequiredService<Func<string>>()();
        InitializeScripting(Path.Join(path, "Server/startup.js"));
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
        _eventsSubscribers.Clear();
        StartScripting();
    }

    public int GetPriority() => int.MaxValue;
}