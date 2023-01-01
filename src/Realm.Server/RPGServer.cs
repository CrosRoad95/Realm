using Realm.Domain.Interfaces;
using Realm.Domain.Registries;

namespace Realm.Server;

public partial class RPGServer : IInternalRPGServer, IRPGServer
{
    private readonly MtaServer<Player> _server;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger _logger;
    private readonly ECS _ecs;

    public ECS ECS => _ecs;
    public event Action<Entity>? PlayerJoined;

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
                    services.AddSingleton(this);
                    services.AddSingleton<EntityByStringIdCollection>();
                    services.AddSingleton<ECS>();
                    services.AddSingleton<IEntityByElement>(x => x.GetRequiredService<ECS>());
                    services.AddSingleton<SeederServerBuilder>();
                    services.AddSingleton<VehicleUpgradeByStringCollection>();
                    services.AddSingleton<ItemsRegistry>();

                    // Services
                    services.AddSingleton<RPGCommandService>();
                    services.AddSingleton<ISignInService, SignInService>();
                    services.AddSingleton<ILoadAndSaveService, LoadAndSaveService>();

                    // Player specific
                    services.AddTransient<DiscordUser>();

                    services.AddTransient<ServicesComponent>();
                    services.AddTransient<IEntityFactory, EntityFactory>();

                    if (modules != null)
                        foreach (var module in modules)
                        {
                            services.AddSingleton(module);
                            module.Configure(services);
                        }
                });
            }
        );

        _ecs = GetRequiredService<ECS>();
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
        var element = (Element)elementHandle.GetElement();
        _server.AssociateElement(element);
    }

    private void HandlePlayerJoined(Player player)
    {
        var playerEntity = _ecs.CreateEntity("Player " + player.Name, Entity.PlayerTag);
        playerEntity.AddComponent(new PlayerElementComponent(player));
        PlayerJoined?.Invoke(playerEntity);
    }

    public TService GetRequiredService<TService>() where TService: notnull
    {
        return _server.GetRequiredService<TService>();
    }

    private Task HandleGuiFilesChanged()
    {
        foreach (var entity in _ecs.Entities)
        {
            var guiComponents = entity.GetComponents().OfType<GuiComponent>().ToList();
            foreach (var guiComponent in guiComponents)
            {
                guiComponent.Close();
                entity.DetachComponent(guiComponent);
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
        await GetRequiredService<ILoadAndSaveService>().LoadAll();

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
}