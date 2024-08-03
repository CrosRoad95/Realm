namespace RealmCore.Server.Modules.Server;

internal sealed class ServerLifecycle : IHostedService
{
    private readonly ILogger<ServerLifecycle> _logger;
    private readonly IOptions<GameplayOptions> _gameplayOptions;
    private readonly RealmCommandService _realmCommandService;
    private readonly IElementCollection _elementCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly MtaServer _mtaServer;
    private readonly ScopedCollisionShapeBehaviour _scopedCollisionShapeBehaviour;
    private readonly IRealmResourcesProvider _realmResourcesProvider;

    public ServerLifecycle(ILogger<ServerLifecycle> logger, IOptions<GameplayOptions> gameplayOptions, RealmCommandService realmCommandService, IElementCollection elementCollection, IServiceProvider serviceProvider, MtaServer mtaServer, CollisionShapeBehaviour collisionShapeBehaviour, ScopedCollisionShapeBehaviour scopedCollisionShapeBehaviour, IResourceServer resourceServer, IRealmResourcesProvider realmResourcesProvider)
    {
        _logger = logger;
        _gameplayOptions = gameplayOptions;
        _realmCommandService = realmCommandService;
        _elementCollection = elementCollection;
        _serviceProvider = serviceProvider;
        _mtaServer = mtaServer;
        _scopedCollisionShapeBehaviour = scopedCollisionShapeBehaviour;
        _realmResourcesProvider = realmResourcesProvider;
        _mtaServer.AddResourceServer(resourceServer);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CultureInfo.CurrentCulture = _gameplayOptions.Value.Culture;
        CultureInfo.CurrentUICulture = _gameplayOptions.Value.Culture;

        _logger.LogInformation("Server started.");
        _logger.LogInformation("Found resources: {resourcesCount}", _realmResourcesProvider.Count);
        _logger.LogInformation("Created commands: {commandsCount}", _realmCommandService.Count);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Server stopping.");
        int i = 0;

        foreach (var element in _elementCollection.GetAll())
        {
            try
            {
                if (element is RealmVehicle vehicle)
                {
                    await vehicle.GetRequiredService<IElementSaveService>().Save(CancellationToken.None);
                    i++;
                }
                else if (element is RealmPlayer player)
                {
                    await player.GetRequiredService<IElementSaveService>().Save(CancellationToken.None);
                    i++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save element.");
            }
        }

        _logger.LogInformation("Server stopped, saved: {savedElementsCount} elements.", i);
        await Task.Delay(500, CancellationToken.None);
    }

    public static readonly ActivitySource Activity = new("RealmCore.ServerLifecycle", "1.0.0");
}