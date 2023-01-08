using Realm.Domain.Interfaces;
using Realm.Domain.Registries;
using Realm.Server.Logic;

namespace Realm.Server;

public partial class RPGServer : IRPGServer
{
    private readonly MtaServer<Player> _server;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public event Action? ServerStarted;

    public RPGServer(RealmConfigurationProvider realmConfigurationProvider, List<IModule> modules, Action<ServerBuilder>? configureServerBuilder = null)
    {
        _server = MtaServer.CreateWithDiSupport<Player>(
            builder =>
            {
                builder.AddLogic<PlayersLogic>();
                builder.AddLogic<GuisLogic>();
                builder.ConfigureServer(realmConfigurationProvider);
                configureServerBuilder?.Invoke(builder);

                builder.ConfigureServices(services =>
                {
                    // Common
                    services.AddSingleton(realmConfigurationProvider);
                    services.AddSingleton((IRPGServer)this);
                    services.AddSingleton(this);
                    services.AddSingleton<EntityByStringIdCollection>();
                    services.AddSingleton<ECS>();
                    services.AddSingleton<IEntityByElement>(x => x.GetRequiredService<ECS>());
                    services.AddSingleton<SeederServerBuilder>();
                    services.AddSingleton<ItemsRegistry>();
                    services.AddSingleton<VehicleUpgradeRegistry>();

                    // Services
                    services.AddSingleton<RPGCommandService>();
                    services.AddSingleton<ISignInService, SignInService>();
                    services.AddTransient<ISaveService, SaveService>();
                    services.AddTransient<ILoadService, LoadService>();

                    // Player specific
                    services.AddTransient<DiscordUser>();

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

        _serviceProvider = GetRequiredService<IServiceProvider>();
        _logger = GetRequiredService<ILogger>().ForContext<RPGServer>();
        _logger.Information("Starting server:");

        _logger.Information("Modules: {modules}", string.Join(", ", modules.Select(x => x.Name)));

        var serverListConfiguration = realmConfigurationProvider.GetRequired<ServerListConfiguration>("ServerList");
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;
    }

    public void AssociateElement(IElementHandle elementHandle)
    {
        var element = (Element)elementHandle.GetElement();
        _server.AssociateElement(element);
    }

    public TService GetRequiredService<TService>() where TService: notnull
    {
        return _server.GetRequiredService<TService>();
    }
    
    public object GetRequiredService(Type type)
    {
        return _serviceProvider.GetRequiredService(type);
    }

    public async Task Start()
    {
        await GetRequiredService<IDb>().MigrateAsync();
        await BuildFromSeedFiles();
        var modules = GetRequiredService<IEnumerable<IModule>>().ToArray();
        var serviceProvider = _server.GetRequiredService<IServiceProvider>();
        foreach (var module in modules)
            module.Init(serviceProvider);

        _server.Start();
        await GetRequiredService<ILoadService>().LoadAll();

        ServerStarted?.Invoke();
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