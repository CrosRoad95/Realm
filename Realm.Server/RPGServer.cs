using Realm.Scripting.Classes;
using Realm.Server.Scripting;
using Realm.Server.Scripting.Events;

namespace Realm.Server;

public partial class RPGServer : IRPGServer
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
    private readonly MtaServer<RPGPlayer> _server;
    private readonly SlipeServerConfiguration _serverConfiguration;
    private readonly ScriptingConfiguration _scriptingConfiguration;
    private readonly ILogger _logger;
    private readonly EventFunctions _eventFunctions;
    private readonly ElementFunctions _elementFunctions;
    private readonly IEnumerable<IModule> _modules;
    private readonly Dictionary<string, Func<LuaEvent, Task<object?>>> _eventHandlers = new();

    public event Action<Player>? PlayerJoined;
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
        _serverConfiguration = configurationProvider.Get<SlipeServerConfiguration>("server");
        _scriptingConfiguration = configurationProvider.Get<ScriptingConfiguration>("scripting");
        _server = MtaServer.CreateWithDiSupport<RPGPlayer>(
            builder =>
            {
                builder.ConfigureServer(configurationProvider.Configuration, basePath);
                if (configureServerBuilder != null)
                    configureServerBuilder(builder);

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(configurationProvider);
                    services.AddSingleton(logger);
                    services.AddSingleton(this);
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

        var serverListConfiguration = configurationProvider.Get<ServerListConfiguration>("serverList");
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;
        _server.PlayerJoined += e => PlayerJoined?.Invoke(e);
        _server.LuaEventTriggered += Server_LuaEventTriggered;

        var startup = _server.GetRequiredService<Startup>();
        _eventFunctions = _server.GetRequiredService<EventFunctions>();
        _elementFunctions = _server.GetRequiredService<ElementFunctions>();

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
                        (e.Player as RPGPlayer).TriggerClientEvent($"{e.Name}Response", response);
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
        await _eventFunctions.InvokeEvent("onPlayerJoin", new PlayerJoinedEvent
        {
            Player = player,
        });
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        // Events
        _eventFunctions.RegisterEvent("onPlayerJoin");
        _eventFunctions.RegisterEvent("onFormSubmit");
        _eventFunctions.RegisterEvent("onPlayerLogin");
        _eventFunctions.RegisterEvent("onPlayerLogout");
        _eventFunctions.RegisterEvent("onPlayerSpawn");

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

    private async void Server_LuaEventTriggered(LuaEvent luaEvent)
    {

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
}