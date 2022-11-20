using Realm.Server.Components;
using Realm.Server.ElementCollections;
using Realm.Server.Elements.CollisionShapes;
using Realm.Server.Elements.Variants;

namespace Realm.Server;

public partial class RPGServer : IRPGServer, IReloadable
{
    private readonly SemaphoreSlim _semaphore = new(0);
    private readonly MtaServer<RPGPlayer> _server;
    private readonly SlipeServerConfiguration _serverConfiguration;
    private readonly ILogger _logger;
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly ElementScriptingFunctions _elementFunctions;
    private readonly InputScriptingFunctions _inputFunctions;
    private readonly GameplayScriptingFunctions _gameplayFunctions;
    private readonly LocalizationScriptingFunctions _localizationFunctions;
    private readonly IElementCollection _elementCollection;
    private readonly IEnumerable<IModule> _modules;

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
                    // Common
                    services.AddSingleton(configurationProvider);
                    services.AddSingleton(logger);
                    services.AddSingleton(this);
                    services.AddSingleton<IReloadable>(this);
                    services.AddSingleton<IRPGServer>(this);
                    services.AddSingleton<ElementByStringIdCollection>();

                    // Scripting
                    services.AddSingleton<GameplayScriptingFunctions>();
                    services.AddSingleton<LocalizationScriptingFunctions>();
                    services.AddSingleton<ElementScriptingFunctions>();
                    services.AddSingleton<InputScriptingFunctions>();

                    // Services
                    services.AddSingleton<AccountsInUseService>();
                    services.AddSingleton<IAccountsInUseService>(x => x.GetRequiredService<AccountsInUseService>());
                    services.AddSingleton<IDiscordVerificationHandler, DiscordVerificationHandler>();
                    services.AddSingleton<IDiscordUserChangedHandler, DiscordUserChangedHandler>();

                    // Player specific
                    services.AddTransient<DiscordUser>();

                    // Elements
                    services.AddTransient<Spawn>();
                    services.AddTransient<RPGVehicle>();
                    services.AddTransient<RPGBlip>();
                    services.AddTransient<RPGRadarArea>();
                    services.AddTransient<RPGPickup>();
                    services.AddTransient<RPGFraction>();
                    services.AddTransient<RPGCollisionSphere>();
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
        _eventFunctions = _server.GetRequiredService<EventScriptingFunctions>();
        _gameplayFunctions = _server.GetRequiredService<GameplayScriptingFunctions>();
        _localizationFunctions = _server.GetRequiredService<LocalizationScriptingFunctions>();
        _elementFunctions = _server.GetRequiredService<ElementScriptingFunctions>();
        _inputFunctions = _server.GetRequiredService<InputScriptingFunctions>();
        _elementCollection = _server.GetRequiredService<IElementCollection>();

        var _ = Task.Run(startup.StartAsync);

        Console.CancelKeyPress += (sender, args) =>
        {
            _server.Stop();
            _semaphore.Release();
        };

        _server.PlayerJoined += Server_PlayerJoined;
    }

    public void AssociateElement(Element element)
    {
        _server.AssociateElement(element);
    }

    private async void Server_PlayerJoined(RPGPlayer player)
    {
        if (player.ResourceStartingLatch != null)
            await player.ResourceStartingLatch;

        player.ResourceStartingLatch = new();
        using var playerJoinedEvent = new PlayerJoinedEvent(player);
        await _eventFunctions.InvokeEvent(playerJoinedEvent);
    }

    public void InitializeScripting(IScriptingModuleInterface scriptingModuleInterface)
    {
        // Events
        _eventFunctions.RegisterEvent(FormContextEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerJoinedEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerLoggedInEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerLoggedOutEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerSpawnedEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerDiscordConnectedEvent.EventName);
        _eventFunctions.RegisterEvent(DiscordUserChangedEvent.EventName);

        // Functions
        scriptingModuleInterface.AddHostObject("Elements", _elementFunctions, true);
        scriptingModuleInterface.AddHostObject("Input", _inputFunctions, true);
        scriptingModuleInterface.AddHostObject("Gameplay", _gameplayFunctions);
        scriptingModuleInterface.AddHostObject("Localization", _localizationFunctions, true);

        // Classes & Events & Contextes
        scriptingModuleInterface.AddHostType(typeof(Claim));
        scriptingModuleInterface.AddHostType(typeof(RPGPlayer));
        scriptingModuleInterface.AddHostType(typeof(Spawn));
        scriptingModuleInterface.AddHostType(typeof(RPGVehicle));
        scriptingModuleInterface.AddHostType(typeof(RPGVariantBlip));
        scriptingModuleInterface.AddHostType(typeof(RPGRadarArea));
        scriptingModuleInterface.AddHostType(typeof(RPGVariantRadarArea));
        scriptingModuleInterface.AddHostType(typeof(RPGPickup));
        scriptingModuleInterface.AddHostType(typeof(RPGVariantPickup));
        scriptingModuleInterface.AddHostType(typeof(RPGFraction));
        scriptingModuleInterface.AddHostType(typeof(RPGCollisionSphere));
        scriptingModuleInterface.AddHostType(typeof(FormContextEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerJoinedEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerLoggedInEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerLoggedOutEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerSpawnedEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerDiscordConnectedEvent));
        scriptingModuleInterface.AddHostType(typeof(DiscordUserChangedEvent));

        scriptingModuleInterface.AddHostType(typeof(ComponentSystem));
        scriptingModuleInterface.AddHostType(typeof(VehicleFuelComponent));
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