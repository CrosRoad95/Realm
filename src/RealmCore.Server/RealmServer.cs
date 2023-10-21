using RealmCore.Server.Behaviours;
using RealmCore.Server.Logic.Components;

namespace RealmCore.Server;

public class RealmServer : MtaServer, IRealmServer
{
    public Entity Console { get; private set; } = null!;
    public event Action? ServerStarted;

    public RealmServer(IRealmConfigurationProvider realmConfigurationProvider, Action<ServerBuilder>? configureServerBuilder = null) : base(serverBuilder =>
    {
        serverBuilder.AddLogic<PlayersLogic>();
        serverBuilder.AddLogic<VehiclesLogic>();
        serverBuilder.AddLogic<GuiLogic>();
        serverBuilder.AddLogic<ServerLogic>();

        serverBuilder.AddLogic<VehicleUpgradeRegistryLogic>();
        serverBuilder.AddLogic<VehicleEnginesRegistryLogic>();

        serverBuilder.AddLogic<VehicleAccessControllerComponentLogic>();

        serverBuilder.AddBehaviour<PrivateCollisionShapeBehaviour>();

        serverBuilder.ConfigureServer(realmConfigurationProvider);
        configureServerBuilder?.Invoke(serverBuilder);

        serverBuilder.ConfigureServices(services =>
        {
            services.ConfigureRealmServices();
        });
    })
    {
    }

    public override async void Start()
    {
        try
        {
            await StartCore();
        }
        catch(Exception ex)
        {
            var logger = GetRequiredService<ILogger<RealmServer>>();
            logger.LogError(ex, "Failed to start server.");
        }
    }
    public async Task StartCore()
    {
        var logger = GetRequiredService<ILogger<RealmServer>>();
        await GetRequiredService<IDb>().MigrateAsync();
        await GetRequiredService<SeederServerBuilder>().Build();
        await GetRequiredService<ILoadService>().LoadAll();

        var gameplayOptions = GetRequiredService<IOptions<GameplayOptions>>();
        CultureInfo.CurrentCulture = gameplayOptions.Value.Culture;
        CultureInfo.CurrentUICulture = gameplayOptions.Value.Culture;
        var realmCommandService = GetRequiredService<RealmCommandService>();

        base.Start();
        logger.LogInformation("Server started.");
        logger.LogInformation("Found resources: {resourcesCount}", RealmResourceServer._resourceCounter);
        logger.LogInformation("Created commands: {commandsCount}", realmCommandService.Count);
        ServerStarted?.Invoke();
        await Task.Delay(-1);
    }

    public new async Task Stop()
    {
        var logger = GetRequiredService<ILogger<RealmServer>>();
        logger.LogInformation("Server stopping.");
        int i = 0;
        var saveService = GetRequiredService<ISaveService>();

        var ecs = GetRequiredService<IEntityEngine>();
        var entities = ecs.Entities.ToList();
        foreach (var entity in entities)
        {
            try
            {
                if (await saveService.BeginSave(entity))
                    i++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save entity.");
            }
            finally
            {
                entity.Dispose();
            }
        }
        try
        {
            await saveService.Commit();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save entities.");
        }

        await Task.Delay(500);
        base.Stop();
        logger.LogInformation("Server stopped, saved: {savedEntitiesCount} entities.", i);
    }
}