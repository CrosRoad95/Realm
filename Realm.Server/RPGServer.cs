using Realm.Server.Scripting;
using SlipeServer.Packets.Definitions.Lua.ElementRpc.Element;
using SlipeServer.Server.Elements.Enums;
using System.Reflection;

namespace Realm.Server;

public partial class RPGServer : IRPGServer, IReloadable
{
    private readonly string _serverId = "";
    private readonly SemaphoreSlim _semaphore = new(0);
    private readonly MtaServer<RPGPlayer> _server;
    private readonly SlipeServerConfiguration _serverConfiguration;
    private readonly ILogger _logger;
    private readonly EventFunctions _eventFunctions;
    private readonly ElementFunctions _elementFunctions;
    private readonly IElementCollection _elementCollection;
    private readonly IEnumerable<IModule> _modules;
    private readonly RootElement _rootElement;
    private readonly Dictionary<string, Func<LuaEvent, Task<object?>>> _eventHandlers = new();

    public event Action<Player>? PlayerJoined;
    public event Action? ServerReloaded;
    public string MapName
    {
        get => _server.MapName;
        set => _server.MapName = value;
    }

    public string GameType
    {
        get => _server.GameType;
        set => _server.GameType = value;
    }

    public RPGServer(ConfigurationProvider configurationProvider, ILogger logger, IEnumerable<IModule> modules, Action<ServerBuilder>? configureServerBuilder = null, string? basePath = null)
    {
        _modules = modules;
        _logger = logger.ForContext<IRPGServer>();
        _serverConfiguration = configurationProvider.Get<SlipeServerConfiguration>("Server");
        _server = MtaServer.CreateWithDiSupport<RPGPlayer>(
            builder =>
            {
                builder.ConfigureServer(configurationProvider.Configuration, basePath);
                configureServerBuilder?.Invoke(builder);

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(configurationProvider);
                    services.AddSingleton(logger);
                    services.AddSingleton(this);
                    services.AddSingleton<IReloadable>(this);
                    services.AddSingleton<IRPGServer>(this);
                    services.AddSingleton<ElementFunctions>();

                    if (modules != null)
                        foreach (var module in modules)
                        {
                            services.AddSingleton(module);
                            module.Configure(services);
                        }
                });
            }
        );

        var serverListConfiguration = configurationProvider.Get<ServerListConfiguration>("ServerList");
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;
        _server.PlayerJoined += e => PlayerJoined?.Invoke(e);

        var startup = _server.GetRequiredService<Startup>();
        _eventFunctions = _server.GetRequiredService<EventFunctions>();
        _elementFunctions = _server.GetRequiredService<ElementFunctions>();
        _elementCollection = _server.GetRequiredService<IElementCollection>();
        _rootElement = _server.GetRequiredService<RootElement>();

        var _ = Task.Run(startup.StartAsync);

        Console.CancelKeyPress += (sender, args) =>
        {
            _server.Stop();
            _semaphore.Release();
        };

        _server.PlayerJoined += Server_PlayerJoined;


        _server.LuaEventTriggered += async e =>
        {
            foreach (var pair in _eventHandlers)
            {
                if (pair.Key == e.Name)
                {
                    var response = await pair.Value(e);
                    if(response != null)
                    {
                        ((RPGPlayer)e.Player).TriggerClientEvent($"{e.Name}Response", response);
                    }
                }
            }
        };
    }

    public void AssociateElement(Element element)
    {
        _server.AssociateElement(element);
    }

    public void AddEventHandler(string eventName, Func<LuaEvent, Task<object?>> callback)
    {
        if (_eventHandlers.ContainsKey(eventName))
            throw new Exception("Event already handled");
        _eventHandlers[eventName] = callback;
    }

    private async void Server_PlayerJoined(RPGPlayer player)
    {
        if (player.ResourceStartingLatch != null)
            await player.ResourceStartingLatch;

        player.ResourceStartingLatch = new();
        await _eventFunctions.InvokeEvent(new PlayerJoinedEvent
        {
            Player = player,
        });
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        // Events
        _eventFunctions.RegisterEvent(FormContext.EventName);
        _eventFunctions.RegisterEvent(PlayerJoinedEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerLoggedInEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerLoggedOutEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerSpawned.EventName);

        // Functions
        scriptingModuleInterface.AddHostObject("Elements", _elementFunctions, true);

        // Classes & Events & Contextes
        scriptingModuleInterface.AddHostType(typeof(Claim));
        scriptingModuleInterface.AddHostType(typeof(RPGPlayer));
        scriptingModuleInterface.AddHostType(typeof(Spawn));
        scriptingModuleInterface.AddHostType(typeof(FormContext));
        scriptingModuleInterface.AddHostType(typeof(PlayerJoinedEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerLoggedInEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerLoggedOutEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerSpawned));
    }

    public TService GetRequiredService<TService>() where TService: notnull
    {
        return _server.GetRequiredService<TService>();
    }

    public async Task Start()
    {
        _logger.Information("Initializing modules: {modules}", string.Join(", ", _modules.Select(x => x.Name)));
        var serviceProvider = _server.GetRequiredService<IServiceProvider>();
        foreach (var module in _modules)
            module.Init(serviceProvider);

        var scriptingModule = _modules.FirstOrDefault(x => x.Name == "Scripting");
        if (scriptingModule != null)
            InitializeScripting(scriptingModule.GetInterface<IScriptingModuleInterface>());

        foreach (var module in _modules)
            module.PostInit(serviceProvider);

        _logger.Information("Server started at port: {port}", _serverConfiguration.Port);
        _server.Start();
        await _semaphore.WaitAsync();
    }

    private void RemoveAllElements()
    {
        foreach (var spawn in _elementFunctions.GetCollectionByType("spawn").Cast<Spawn>())
            if(!spawn.IsPersistant() && _elementFunctions.IsElement(spawn))
                _elementFunctions.DestroyElement(spawn);

    }

    public Task Reload()
    {
        var players = _elementCollection.GetByType<Player>().Cast<RPGPlayer>();
        foreach (var player in players)
            player.Reset();
        foreach (var player in players)
            Server_PlayerJoined(player);

        RemoveAllElements();
        ServerReloaded?.Invoke();
        return Task.CompletedTask;
    }

    public int GetPriority() => 0;
}