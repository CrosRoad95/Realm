global using ILogger = Microsoft.Extensions.Logging.ILogger;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace RealmCore.Server.Modules.Server;

public class RealmServer<TRealmPlayer> : MtaServer<TRealmPlayer> where TRealmPlayer: RealmPlayer
{
    public event Action? ServerStarted;

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = Instantiate<TRealmPlayer>();
        player.Client = new Client<TRealmPlayer>(binaryAddress, netWrapper, player);
        return player.Client;
    }

    public RealmServer(IRealmConfigurationProvider realmConfigurationProvider, Action<ServerBuilder>? configureServerBuilder = null, Action<ServerBuilder>? postConfigureServerBuilder = null) : base(serverBuilder =>
    {
        serverBuilder.ConfigureServer(realmConfigurationProvider);
        configureServerBuilder?.Invoke(serverBuilder);

        serverBuilder.ConfigureServices(services =>
        {
            services.ConfigureRealmServices();
        });

        postConfigureServerBuilder?.Invoke(serverBuilder);
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
        {
            using var activity = Activity.StartActivity(nameof(StartCore));

            var logger = GetRequiredService<ILogger<RealmServer>>();

            {
                using var activityMigrate = Activity.StartActivity("Database migration");
                await GetRequiredService<IDb>().MigrateAsync();
            }

            {
                using var activityLoadFraction = Activity.StartActivity("Load fractions");
                await GetRequiredService<IFractionService>().LoadFractions(cancellationToken);
            }

            {
                using var activitySeeder = Activity.StartActivity("Seeder");
                using var seederServerBuilder = GetRequiredService<SeederServerBuilder>();
                await seederServerBuilder.Build(cancellationToken);
            }

            {
                using var activityLoadAll = Activity.StartActivity("Load all");

                await GetRequiredService<ILoadService>().LoadAll(cancellationToken);
            }

            var gameplayOptions = GetRequiredService<IOptions<GameplayOptions>>();
            CultureInfo.CurrentCulture = gameplayOptions.Value.Culture;
            CultureInfo.CurrentUICulture = gameplayOptions.Value.Culture;
            var realmCommandService = GetRequiredService<RealmCommandService>();

            base.Start();
            logger.LogInformation("Server started.");
            logger.LogInformation("Found resources: {resourcesCount}", RealmResourceServer._resourceCounter);
            logger.LogInformation("Created commands: {commandsCount}", realmCommandService.Count);
            ServerStarted?.Invoke();
        }
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


    public static readonly ActivitySource Activity = new("RealmCore.RealmServer", "1.0.0");
}

public class RealmServer : RealmServer<RealmPlayer>
{
    public RealmServer(IRealmConfigurationProvider realmConfigurationProvider, Action<ServerBuilder>? configureServerBuilder = null) : base(realmConfigurationProvider, configureServerBuilder)
    {
    }
}