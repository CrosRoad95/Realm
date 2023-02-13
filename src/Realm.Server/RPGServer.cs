using Realm.Domain.IdGenerators;
using Realm.Domain.Registries;
using Realm.Module.Discord;
using Realm.Module.Grpc;
using Realm.Module.WebApp;
using Realm.Server.Logic;
using SlipeServer.Server.Elements.IdGeneration;

namespace Realm.Server;

internal sealed class RPGServer : IRPGServer
{
    private readonly MtaServer _server;
    private readonly ILogger<RPGServer> _logger;

    public event Action? ServerStarted;

    internal MtaServer MtaServer => _server;
    public RPGServer(IRealmConfigurationProvider realmConfigurationProvider, Action<ServerBuilder>? configureServerBuilder = null)
    {
        _server = new MtaServer(
            builder =>
            {
                builder.AddLogic<PlayersLogic>();
                builder.AddLogic<GuisLogic>();
                builder.AddLogic<StartupLogic>();
                builder.AddLogic<ChatLogic>();
                builder.AddLogic<ModulesLogic>();
                builder.ConfigureServer(realmConfigurationProvider);
                configureServerBuilder?.Invoke(builder);

                builder.ConfigureServices(ConfigureServices);
            }
        );

        _logger = GetRequiredService<ILogger<RPGServer>>();
        _logger.LogInformation("Starting server:");
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Common
        services.AddSingleton((IRPGServer)this);
        services.AddSingleton(this);
        services.AddSingleton<SeederServerBuilder>();
        services.AddSingleton<ItemsRegistry>();
        services.AddSingleton<VehicleUpgradeRegistry>();
        services.AddSingleton<LevelsRegistry>();

        // Services
        services.AddSingleton<RPGCommandService>();
        services.AddSingleton<ISignInService, SignInService>();
        services.AddTransient<ISaveService, SaveService>();
        services.AddTransient<ILoadService, LoadService>();
        services.AddTransient<IVehiclesService, VehiclesService>();
        services.AddTransient<IGroupService, GroupService>();
        services.AddTransient<IFractionService, FractionService>();

        services.AddTransient<IEntityFactory, EntityFactory>();

        services.AddSingleton<IElementIdGenerator, RangedCollectionBasedElementIdGenerator>(x =>
            new RangedCollectionBasedElementIdGenerator(x.GetRequiredService<IElementCollection>(), IdGeneratorConstants.PlayerIdStart, IdGeneratorConstants.PlayerIdStop)
        );

        services.AddGrpcModule();
        services.AddDiscordModule();
        services.AddWebAppModule();
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
        _logger.LogInformation("Server started.");
    }

    private async Task BuildFromSeedFiles()
    {
        var seedServerBuilder = GetRequiredService<SeederServerBuilder>();
        await seedServerBuilder.Build();
    }

    public async Task Stop()
    {
        _logger.LogInformation("Server stopping.");
        int i = 0;
        var saveService = GetRequiredService<ISaveService>();

        var ecs = GetRequiredService<ECS>();
        var entities = ecs.Entities.ToList();
        foreach (var entity in entities)
        {
            try
            {
                if (await saveService.Save(entity))
                    i++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save entity.");
            }
            finally
            {
                ecs.Destroy(entity);
            }
        }
        try
        {
            await saveService.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save entities.");
        }

        await Task.Delay(500);
        _server.Stop();
        _logger.LogInformation("Server stopped, saved: {amount} entities.", i);
    }
}