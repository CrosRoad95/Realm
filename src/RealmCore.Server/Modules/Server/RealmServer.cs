global using ILogger = Microsoft.Extensions.Logging.ILogger;
using Microsoft.AspNetCore.Hosting.Server;
using SlipeServer.Server.Behaviour;

[assembly: InternalsVisibleTo("RealmCore.TestingTools")]
[assembly: InternalsVisibleTo("RealmCore.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace RealmCore.Server.Modules.Server;

public sealed class ServerLoadingException : Exception
{
    public ServerLoadingException(string message) : base(message)
    {

    }
}

public class RealmServer<TRealmPlayer> : MtaServer<TRealmPlayer> where TRealmPlayer: RealmPlayer
{
    public event Action? ServerStarted;

    protected override IClient CreateClient(uint binaryAddress, INetWrapper netWrapper)
    {
        var player = Instantiate<TRealmPlayer>();
        player.Client = new Client<TRealmPlayer>(binaryAddress, netWrapper, player);
        return player.Client;
    }

    public RealmServer(IConfiguration configuration, Action<ServerBuilder>? configureServerBuilder = null, Action<ServerBuilder>? postConfigureServerBuilder = null) : base(serverBuilder =>
    {
        serverBuilder.ConfigureServer(configuration);
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
            Instantiate<CollisionShapeBehaviour>(); // TODO:

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
                var serverLoaders = GetRequiredService<IEnumerable<IServerLoader>>();
                using var serverLoadersActivity = Activity.StartActivity("Loading server");
                bool anyFailed = false;
                foreach (var item in serverLoaders)
                {
                    using var serverLoaderActivity = Activity.StartActivity($"Loading {item}");
                    try
                    {
                        await item.Load();
                    }
                    catch(Exception ex)
                    {
                        anyFailed = true;
                        serverLoaderActivity?.SetStatus(ActivityStatusCode.Error);
                        logger.LogError(ex, "Failed to load server loader.");
                    }
                }
                if (anyFailed)
                {
                    throw new ServerLoadingException("Server failed to load.");
                }
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

        var elementCollection = GetRequiredService<IElementCollection>();
        foreach (var element in elementCollection.GetAll())
        {
            try
            {
                if(element is RealmVehicle vehicle)
                {
                    await vehicle.GetRequiredService<ISaveService>().Save();
                    i++;
                }
                else if(element is RealmPlayer player)
                {
                    await player.GetRequiredService<ISaveService>().Save();
                    i++;
                }
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
    public RealmServer(IConfiguration configuration, Action<ServerBuilder>? configureServerBuilder = null) : base(configuration, configureServerBuilder)
    {
    }
}