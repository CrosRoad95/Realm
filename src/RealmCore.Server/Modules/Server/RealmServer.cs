global using ILogger = Microsoft.Extensions.Logging.ILogger;
using SlipeServer.Net.Wrappers;

[assembly: InternalsVisibleTo("RealmCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace RealmCore.Server.Modules.Server;

public class MtaDiPlayerServerTempFix<TPlayer> : MtaServer<TPlayer> where TPlayer : Player
{
    public MtaDiPlayerServerTempFix(Action<ServerBuilder> builderAction) : base(builderAction) { }

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = Instantiate<TPlayer>();
        player.Client = new Client<TPlayer>(binaryAddress, netWrapper, player);
        return player.Client;
    }
}

public class RealmServer : MtaDiPlayerServerTempFix<RealmPlayer>
{
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
        catch (Exception ex)
        {
            var logger = GetRequiredService<ILogger<RealmServer>>();
            logger.LogError(ex, "Failed to start server.");
        }
    }
    public async Task StartCore(CancellationToken cancellationToken = default)
    {
        var logger = GetRequiredService<ILogger<RealmServer>>();
        await GetRequiredService<IDb>().MigrateAsync();
        await GetRequiredService<IFractionService>().LoadFractions(cancellationToken);
        await GetRequiredService<SeederServerBuilder>().Build(cancellationToken);
        await GetRequiredService<ILoadService>().LoadAll(cancellationToken);

        var gameplayOptions = GetRequiredService<IOptions<GameplayOptions>>();
        CultureInfo.CurrentCulture = gameplayOptions.Value.Culture;
        CultureInfo.CurrentUICulture = gameplayOptions.Value.Culture;
        var realmCommandService = GetRequiredService<RealmCommandService>();

        base.Start();
        logger.LogInformation("Server started.");
        logger.LogInformation("Found resources: {resourcesCount}", RealmResourceServer._resourceCounter);
        logger.LogInformation("Created commands: {commandsCount}", realmCommandService.Count);
        ServerStarted?.Invoke();
        await Task.Delay(-1, cancellationToken);
    }

    public new async Task Stop()
    {
        var logger = GetRequiredService<ILogger<RealmServer>>();
        logger.LogInformation("Server stopping.");
        int i = 0;
        var saveService = GetRequiredService<ISaveService>();

        var elementCollection = GetRequiredService<IElementCollection>();
        foreach (var element in elementCollection.GetAll())
        {
            try
            {
                if (await saveService.Save(element))
                    i++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save element.");
            }
            finally
            {
                element.Destroy();
            }
        }

        await Task.Delay(500);
        base.Stop();
        logger.LogInformation("Server stopped, saved: {savedElementsCount} elements.", i);
    }
}