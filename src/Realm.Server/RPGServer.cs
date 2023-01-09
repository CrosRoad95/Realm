using Realm.Configuration;
using Realm.Domain.Interfaces;
using Realm.Domain.Registries;
using Realm.Server.Logic;

namespace Realm.Server;

internal sealed class RPGServer : IRPGServer
{
    private readonly MtaServer _server;
    private readonly ILogger _logger;
    private readonly List<IModule> _modules;

    public event Action? ServerStarted;

    public RPGServer(RealmConfigurationProvider realmConfigurationProvider, List<IModule> modules, Action<ServerBuilder>? configureServerBuilder = null)
    {
        _server = new MtaServer(
            builder =>
            {
                builder.AddLogic<PlayersLogic>();
                builder.AddLogic<GuisLogic>();
                builder.AddLogic<ModulesLogic>();
                builder.AddLogic<StartupLogic>();
                builder.ConfigureServer(realmConfigurationProvider);
                configureServerBuilder?.Invoke(builder);

                builder.ConfigureServices(ConfigureServices);
            }
        );

        _logger = GetRequiredService<ILogger>().ForContext<RPGServer>();
        _logger.Information("Starting server:");
        _modules = modules;
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Common
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

        if (_modules != null)
            foreach (var module in _modules)
            {
                services.AddSingleton(module);
                module.Configure(services);
            }
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
    
    public async Task Start()
    {
        await GetRequiredService<IDb>().MigrateAsync();
        await BuildFromSeedFiles();

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