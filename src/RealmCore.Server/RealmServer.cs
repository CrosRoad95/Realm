using SlipeServer.Net.Wrappers;

namespace RealmCore.Server;

public static class ServerBuilderExtensions
{
    public static T InstantiateScoped<T>(this MtaServer mtaServer, params object[] parameters)
    {
        var scope = mtaServer.Services.CreateScope();
        return ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider, parameters);
    }

    public static void InstantiatePersistentScoped<T>(this ServerBuilder serverBuilder, params object[] parameters)
    {
        object[] parameters2 = parameters;
        serverBuilder.AddBuildStep(delegate (MtaServer server)
        {
            server.InstantiateScoped<T>(parameters2);
        });
    }

    public static void AddScopedLogic<T>(this ServerBuilder serverBuilder, params object[] parameters)
    {
        serverBuilder.InstantiatePersistent<T>(parameters);
    }
}

public class MtaDiPlayerServerTempFix<TPlayer> : MtaServer<TPlayer> where TPlayer : Player
{
    public MtaDiPlayerServerTempFix(Action<ServerBuilder> builderAction) : base(builderAction) { }

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = this.Instantiate<TPlayer>();
        player.Client = new Client<TPlayer>(binaryAddress, netWrapper, player);
        return player.Client;
    }
}


public class RealmServer : MtaDiPlayerServerTempFix<PlayerElementComponent>, IRealmServer
{
    public Entity Console { get; private set; } = null!;
    public event Action? ServerStarted;

    public RealmServer(IRealmConfigurationProvider realmConfigurationProvider, Action<ServerBuilder>? configureServerBuilder = null) : base(serverBuilder =>
    {
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

        var entityEngine = GetRequiredService<IEntityEngine>();
        var entities = entityEngine.Entities.ToList();
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