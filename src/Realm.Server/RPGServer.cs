using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Realm.Domain;
using Realm.Domain.Inventory;
using Realm.Domain.New;
using Realm.Domain.Sessions;
using Realm.Interfaces.Common;
using Realm.Server.Interfaces;
using Realm.Server.Scripting;
using Realm.Server.Scripting.Events;
using static Realm.Domain.Upgrades.VehicleUpgrade;

namespace Realm.Server;

public partial class RPGServer : IInternalRPGServer, IRPGServer, IReloadable
{
    private readonly MtaServer<Player> _server;
    private readonly EventScriptingFunctions _eventFunctions;
    private readonly ElementScriptingFunctions _elementFunctions;
    private readonly InputScriptingFunctions _inputFunctions;
    private readonly GameplayScriptingFunctions _gameplayFunctions;
    private readonly LocalizationScriptingFunctions _localizationFunctions;
    private readonly ServerScriptingFunctions _serverScriptingFunctions;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger _logger;

    public event Action<Player>? PlayerJoined;
    public event Action? ServerReloaded;

    private readonly List<Entity> _entities = new();
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

    public RPGServer(RealmConfigurationProvider realmConfigurationProvider, List<IModule> modules, Action<ServerBuilder>? configureServerBuilder = null)
    {
        _server = MtaServer.CreateWithDiSupport<Player>(
            builder =>
            {
                builder.ConfigureServer(realmConfigurationProvider);
                configureServerBuilder?.Invoke(builder);

                builder.ConfigureServices(services =>
                {
                    // Common
                    services.AddSingleton(realmConfigurationProvider);
                    services.AddSingleton((IReloadable)this);
                    services.AddSingleton((Interfaces.IInternalRPGServer)this);
                    services.AddSingleton<EntityByStringIdCollection>();
                    services.AddSingleton<SeederServerBuilder>();
                    services.AddSingleton<VehicleUpgradeByStringCollection>();

                    // Scripting
                    services.AddSingleton<GameplayScriptingFunctions>();
                    services.AddSingleton<LocalizationScriptingFunctions>();
                    services.AddSingleton<ElementScriptingFunctions>();
                    services.AddSingleton<InputScriptingFunctions>();
                    services.AddSingleton<ServerScriptingFunctions>();

                    // Services
                    services.AddSingleton<RPGCommandService>();
                    services.AddSingleton((Func<IServiceProvider, IReloadable>)(x => x.GetRequiredService<RPGCommandService>()));

                    // Player specific
                    services.AddTransient<DiscordUser>();

                    services.AddSingleton<ServicesComponent>();

                    if (modules != null)
                        foreach (var module in modules)
                        {
                            services.AddSingleton(module);
                            module.Configure(services);
                        }
                });
            }
        );

        _logger = GetRequiredService<ILogger>().ForContext<RPGServer>();

        _logger.Information("Startin server:");
        _logger.Information("Modules: {modules}", string.Join(", ", modules.Select(x => x.Name)));

        var serverListConfiguration = realmConfigurationProvider.GetRequired<ServerListConfiguration>("ServerList");
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;
        _server.PlayerJoined += e => PlayerJoined?.Invoke(e);

        _eventFunctions = _server.GetRequiredService<EventScriptingFunctions>();
        _gameplayFunctions = _server.GetRequiredService<GameplayScriptingFunctions>();
        _localizationFunctions = _server.GetRequiredService<LocalizationScriptingFunctions>();
        _elementFunctions = _server.GetRequiredService<ElementScriptingFunctions>();
        _inputFunctions = _server.GetRequiredService<InputScriptingFunctions>();
        _serverScriptingFunctions = _server.GetRequiredService<ServerScriptingFunctions>();
        _elementCollection = _server.GetRequiredService<IElementCollection>();

        _server.PlayerJoined += HandlePlayerJoined;
    }

    public void AssociateElement(IElementHandle elementHandle)
    {
        _server.AssociateElement((Element)elementHandle.GetElement());
    }

    private async void HandlePlayerJoined(Player player)
    {
        var playerEntity = CreateEntity("Player " + player.Name);
        playerEntity.AddComponent(new PlayerElementCompoent(player));
        using var playerJoinedEvent = new PlayerJoinedEvent(playerEntity);
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
        _eventFunctions.RegisterEvent(PlayerAFKStateChangedEvent.EventName);
        _eventFunctions.RegisterEvent(VehicleSpawnedEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerDailyVisitEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerSessionStartedEvent.EventName);
        _eventFunctions.RegisterEvent(PlayerSessionStoppedEvent.EventName);
        _eventFunctions.RegisterEvent(ServerReloadedEvent.EventName);

        // Functions
        scriptingModuleInterface.AddHostObject("Elements", _elementFunctions, true);
        scriptingModuleInterface.AddHostObject("Input", _inputFunctions, true);
        scriptingModuleInterface.AddHostObject("Gameplay", _gameplayFunctions);
        scriptingModuleInterface.AddHostObject("Localization", _localizationFunctions, true);
        scriptingModuleInterface.AddHostObject("Server", _serverScriptingFunctions, true);

        // Classes & Events & Contextes
        scriptingModuleInterface.AddHostType(typeof(Player));
        scriptingModuleInterface.AddHostType(typeof(FormContextEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerJoinedEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerLoggedInEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerLoggedOutEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerSpawnedEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerAFKStateChangedEvent));
        scriptingModuleInterface.AddHostType(typeof(VehicleSpawnedEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerDailyVisitEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerSessionStartedEvent));
        scriptingModuleInterface.AddHostType(typeof(PlayerSessionStoppedEvent));
        scriptingModuleInterface.AddHostType(typeof(ServerReloadedEvent));

        scriptingModuleInterface.AddHostType(typeof(VehicleFuelComponent));
        scriptingModuleInterface.AddHostType(typeof(MileageCounterComponent));
        scriptingModuleInterface.AddHostType(typeof(StatisticsCounterComponent));
        scriptingModuleInterface.AddHostType(typeof(DailyVisitsCounterComponent));
        scriptingModuleInterface.AddHostType(typeof(LastPositionComponent));
        scriptingModuleInterface.AddHostType(typeof(VehicleUpgradeComponent));

        scriptingModuleInterface.AddHostType(typeof(FractionSession));

        scriptingModuleInterface.AddHostType(typeof(Domain.Upgrades.VehicleUpgrade));
        scriptingModuleInterface.AddHostType(typeof(UpgradeDescription));

        scriptingModuleInterface.AddHostType(typeof(InventorySystem));
        scriptingModuleInterface.AddHostType(typeof(PlayerItem));

        // New Api
        scriptingModuleInterface.AddHostType(typeof(Entity));
        scriptingModuleInterface.AddHostType(typeof(TestComponent));
        scriptingModuleInterface.AddHostType(typeof(PlayerElementCompoent));

    }

    public TService GetRequiredService<TService>() where TService: notnull
    {
        return _server.GetRequiredService<TService>();
    }

    public async Task DoReload()
    {
        Save();
        var reloadable = GetRequiredService<IEnumerable<IReloadable>>();
        foreach (var item in reloadable.OrderBy(x => x.GetPriority()))
            await item.Reload();

        var players = _elementCollection.GetByType<Player>();

        foreach (var player in players)
            HandlePlayerJoined(player);

        using var serverReloadedEvent = new ServerReloadedEvent();
        await _eventFunctions.InvokeEvent(serverReloadedEvent);
    }

    public async Task Save()
    {

    }

    public async Task Start()
    {
        await BuildFromSeedFiles();
        var modules = GetRequiredService<IEnumerable<IModule>>().ToArray();
        var serviceProvider = _server.GetRequiredService<IServiceProvider>();
        foreach (var module in modules)
            module.Init(serviceProvider);

        var scriptingModule = modules.FirstOrDefault(x => x.Name == "Scripting");
        if (scriptingModule != null)
            InitializeScripting(scriptingModule.GetInterface<IScriptingModuleInterface>());

        foreach (var module in modules)
            module.PostInit(serviceProvider);

        _server.Start();
        _logger.Information("Server started.");
    }

    private async Task BuildFromSeedFiles()
    {
        var seedServerBuilder = GetRequiredService<SeederServerBuilder>();
        await seedServerBuilder.Build();
    }

    public async Task Stop()
    {
        _server.Stop(); // TODO: save everything
    }

    private void RemoveAllEntities()
    {
        foreach (var entity in _entities)
            entity.Destroy();
    }

    public Task Reload()
    {
        RemoveAllEntities();

        ServerReloaded?.Invoke();
        return Task.CompletedTask;
    }

    public Entity CreateEntity(string name)
    {
        return new Entity(this, GetRequiredService<ServicesComponent>(), name);
    }

    public int GetPriority() => 40;
}