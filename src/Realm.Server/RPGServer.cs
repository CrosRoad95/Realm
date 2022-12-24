namespace Realm.Server;

public partial class RPGServer : IInternalRPGServer, IRPGServer
{
    private readonly MtaServer<Player> _server;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger _logger;

    public event Action<Entity>? PlayerJoined;

    private readonly List<Entity> _entities = new();
    private readonly Dictionary<Player, Entity> _entityByPlayer = new();

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
                    services.AddSingleton((IInternalRPGServer)this);
                    services.AddSingleton((IRPGServer)this);
                    services.AddSingleton<EntityByStringIdCollection>();
                    services.AddSingleton<SeederServerBuilder>();
                    services.AddSingleton<VehicleUpgradeByStringCollection>();

                    // Services
                    services.AddSingleton<RPGCommandService>();

                    // Player specific
                    services.AddTransient<DiscordUser>();

                    services.AddTransient<ServicesComponent>();
                    services.AddTransient<EntityElementFactory>();

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
        _logger.Information("Starting server:");

        _logger.Information("Modules: {modules}", string.Join(", ", modules.Select(x => x.Name)));

        var serverListConfiguration = realmConfigurationProvider.GetRequired<ServerListConfiguration>("ServerList");
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;

        _elementCollection = _server.GetRequiredService<IElementCollection>();

        _server.PlayerJoined += HandlePlayerJoined;
    }

    public void AssociateElement(IElementHandle elementHandle)
    {
        _server.AssociateElement((Element)elementHandle.GetElement());
    }

    public Entity GetEntityByPlayer(Player player)
    {
        return _entityByPlayer[player];
    }

    private void HandlePlayerJoined(Player player)
    {
        var playerEntity = CreateEntity("Player " + player.Name);
        playerEntity.AddComponent(new PlayerElementComponent(player));
        PlayerJoined?.Invoke(playerEntity);
    }

    public TService GetRequiredService<TService>() where TService: notnull
    {
        return _server.GetRequiredService<TService>();
    }

    public async Task Save()
    {
        foreach (var entity in _entities)
        {
            if(entity.TryGetComponent(out AccountComponent accountComponent))
            {
                ;
            }
        }
    }

    private Task HandleGuiFilesChanged()
    {
        foreach (var entity in _entities)
        {
            var guiComponents = entity.GetComponents().OfType<GuiComponent>().ToList();
            foreach (var guiComponent in guiComponents)
            {
                guiComponent.Close();
                entity.RemoveComponent(guiComponent);
                entity.AddComponent(guiComponent);
            }
        }
        return Task.CompletedTask;
    }

    public async Task Start()
    {
        await BuildFromSeedFiles();
        var modules = GetRequiredService<IEnumerable<IModule>>().ToArray();
        var serviceProvider = _server.GetRequiredService<IServiceProvider>();
        foreach (var module in modules)
            module.Init(serviceProvider);

        GetRequiredService<AgnosticGuiSystemService>().GuiFilesChanged = HandleGuiFilesChanged;

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

    public Entity CreateEntity(string name)
    {
        var newlyCreatedEntity = new Entity(this, GetRequiredService<ServicesComponent>(), name);
        _entities.Add(newlyCreatedEntity);
        newlyCreatedEntity.ComponentAdded += HandleComponentAdded;
        newlyCreatedEntity.Destroyed += HandleEntityDestroyed;
        return newlyCreatedEntity;
    }

    private void HandleEntityDestroyed(Entity entity)
    {
        entity.ComponentAdded -= HandleComponentAdded;
    }

    private void HandleComponentAdded(Component component)
    {
        if(component is PlayerElementComponent playerElementComponent)
        {
            _entityByPlayer[playerElementComponent.Player] = component.Entity;
            component.Entity.Destroyed += HandlePlayerEntityDestroyed;
        }
    }

    private void HandlePlayerEntityDestroyed(Entity playerEntity)
    {
        playerEntity.Destroyed += HandlePlayerEntityDestroyed;
        var playerComponent = playerEntity.GetRequiredComponent<PlayerElementComponent>();
        _entityByPlayer.Remove(playerComponent.Player);
    }
}