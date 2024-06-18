namespace RealmCore.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services) where TCommand : class, IInGameCommand
    {
        services.AddSingleton(new CommandTypeWrapper(typeof(TCommand)));
        services.AddTransient<TCommand>();
        return services;
    }

    public static IServiceCollection AddSeederProvider<TSeederProvider>(this IServiceCollection services) where TSeederProvider : class, ISeederProvider
    {
        services.AddSingleton<ISeederProvider, TSeederProvider>();
        return services;
    }

    public static IServiceCollection AddAsyncSeederProvider<TSeederProvider>(this IServiceCollection services) where TSeederProvider : class, IAsyncSeederProvider
    {
        services.AddSingleton<IAsyncSeederProvider, TSeederProvider>();
        return services;
    }

    public static IServiceCollection AddInGameCommand<TInGameCommand>(this IServiceCollection services) where TInGameCommand : class, IInGameCommand
    {
        services.AddTransient<IInGameCommand>(x => x.GetRequiredService<TInGameCommand>());
        services.AddTransient<TInGameCommand>();
        return services;
    }

    public static IServiceCollection AddPlayerScopedFeature<T1, T2>(this IServiceCollection services)
        where T1 : class, IPlayerFeature
        where T2: class, T1
    {
        services.AddScoped<T1, T2>();
        services.AddScoped<IPlayerFeature>(x => x.GetRequiredService<T1>()); 
        return services;
    }
    
    public static IServiceCollection AddVehicleScopedFeature<T1, T2>(this IServiceCollection services)
        where T1 : class, IVehicleFeature
        where T2: class, T1
    {
        services.AddScoped<T1, T2>();
        services.AddScoped<IVehicleFeature>(x => x.GetRequiredService<T1>()); 
        return services;
    }
    
    public static IServiceCollection AddPlayerJoinedPipeline<T>(this IServiceCollection services) where T : class, IPlayerJoinedPipeline
    {
        services.AddTransient<IPlayerJoinedPipeline, T>();
        return services;
    }

    public static IServiceCollection ConfigureRealmServicesCore(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<HostOptions>(options =>
        {
            options.ServicesStartConcurrently = true;
            options.ServicesStopConcurrently = true;
        });

        services.Configure<GameplayOptions>(configuration.GetSection("Gameplay"));
        services.Configure<ServerListOptions>(configuration.GetSection("ServerList"));
        services.Configure<AssetsOptions>(configuration.GetSection("Assets"));
        services.Configure<BrowserOptions>(configuration.GetSection("Browser"));
        services.Configure<VehicleLoadingOptions>(configuration.GetSection("VehicleLoading"));
        services.Configure<CommandsOptions>(configuration.GetSection("Commands"));

        var connectionString = configuration.GetValue<string>("Database:ConnectionString");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddPersistence<MySqlDb>(db => db.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), options =>
            {
                options.EnableRetryOnFailure(10);
            }));
            var identityConfiguration = configuration.GetSection("Identity").Get<IdentityConfiguration>();
            if (identityConfiguration == null)
                throw new Exception("Identity configuration is null");
            services.AddRealmIdentity<MySqlDb>(identityConfiguration);
        }
        else
        {
            throw new Exception("Database connection string is not configured.");
        }

        services.AddSingleton<HelpCommand>();
        services.AddCommand<SaveCommand>();
        services.AddCommand<ServerInfoCommand>();
        services.AddCommand<ReloadElementsCommand>();

        #region Common
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IAssetEncryptionProvider, AssetEncryptionProvider>();
        services.AddSingleton<IDebounceFactory, DebounceFactory>();
        services.AddTransient<SeederServerBuilder>();
        services.AddSingleton<RealmCommandService>();
        services.AddSingleton<MapIdGenerator>();
        services.AddScoped<ScopedMapIdGenerator>();
        services.AddScoped<PlayerContext>();
        services.AddScoped<VehicleContext>();
        services.AddScoped<ElementContext>();
        #endregion

        #region Registries
        services.AddSingleton<MapsCollection>();
        services.AddSingleton<ItemsCollection>();
        services.AddSingleton<VehicleUpgradesCollection>();
        services.AddSingleton<LevelsCollection>();
        services.AddSingleton<VehicleEnginesCollection>();
        #endregion

        #region Security
        services.AddSingleton<IUsersInUse, UsersInUse>();
        services.AddSingleton<IVehiclesInUse, VehiclesInUse>();
        services.AddSingleton<IAntiCheat, AntiCheat>();
        services.AddHostedService<AntiCheatService>();
        #endregion

        #region Services
        services.AddScoped<IElementSaveService, ElementSaveService>();
        services.AddScoped<IVehicleLoader, VehicleLoader>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IPlayerMapsService, PlayerMapsService>();
        services.AddScoped<IElementSearchService, ElementSearchService>();
        services.AddScoped<IPlayerUserService, PlayersUsersService>();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddSingleton<IGroupsService, GroupsService>();
        services.AddSingleton<IFractionsService, FractionsService>();
        services.AddSingleton<IRewardsService, RewardsService>();
        services.AddSingleton<IFeedbackService, PlayerFeedbackService>();
        services.AddSingleton<ISpawnMarkersService, SpawnMarkersService>();
        services.AddSingleton<INewsService, NewsService>();
        services.AddSingleton<IMoneyHistoryService, MoneyHistoryService>();
        services.AddSingleton<IBrowserGuiService, BrowserGuiService>();
        services.AddSingleton<IMapsService, MapsService>();
        services.AddSingleton<PlayersEventManager>();
        services.AddSingleton<ISchedulerService, SchedulerService>();
        services.AddSingleton<IVehiclesService, VehiclesService>();
        services.AddSingleton<CollisionShapeBehaviour>();
        services.AddSingleton<ScopedCollisionShapeBehaviour>();
        services.AddSingleton<FriendsService>();
        services.AddSingleton<IRealmResourcesProvider, RealmResourcesProvider>();
        services.AddSingleton<IResourceServer>(x => new RealmResourceServer(x.GetRequiredService<BasicHttpServer>(), x.GetRequiredService<IRealmResourcesProvider>()));
        services.AddSingleton<IPlayersNotifications, PlayersNotifications>();
        #endregion

        #region Player features
        services.AddPlayerScopedFeature<IPlayerBrowserFeature, PlayerBrowserFeature>();
        services.AddPlayerScopedFeature<IPlayerAFKFeature, PlayerAFKFeature>();
        services.AddPlayerScopedFeature<IPlayerMoneyFeature, PlayerMoneyFeature>();
        services.AddPlayerScopedFeature<IPlayerUserFeature, PlayerUserFeature>();
        services.AddPlayerScopedFeature<IPlayerDailyVisitsFeature, PlayerDailyVisitsFeature>();
        services.AddPlayerScopedFeature<IPlayerSettingsFeature, PlayerSettingsFeature>();
        services.AddPlayerScopedFeature<IPlayerBansFeature, PlayerBansFeature>();
        services.AddPlayerScopedFeature<IPlayerUpgradesFeature, PlayerUpgradesFeature>();
        services.AddPlayerScopedFeature<IPlayerPlayTimeFeature, PlayerPlayTimeFeature>();
        services.AddPlayerScopedFeature<IPlayerLevelFeature, PlayerLevelFeature>();
        services.AddPlayerScopedFeature<IPlayerLicensesFeature, PlayerLicensesFeature>();
        services.AddPlayerScopedFeature<IPlayerStatisticsFeature, PlayerStatisticsFeature>();
        services.AddPlayerScopedFeature<IPlayerAchievementsFeature, PlayerAchievementsFeature>();
        services.AddPlayerScopedFeature<IPlayerDiscoveriesFeature, PlayerDiscoveriesFeature>();
        services.AddPlayerScopedFeature<IPlayerJobUpgradesFeature, PlayerJobUpgradesFeature>();
        services.AddPlayerScopedFeature<IPlayerJobStatisticsFeature, PlayerJobStatisticsFeature>();
        services.AddPlayerScopedFeature<IPlayerEventsFeature, PlayerEventsFeature>();
        services.AddPlayerScopedFeature<IPlayerSessionsFeature, PlayerSessionsFeature>();
        services.AddPlayerScopedFeature<IPlayerAdminFeature, PlayerAdminFeature>();
        services.AddPlayerScopedFeature<IPlayerGroupsFeature, PlayerGroupsFeature>();
        services.AddPlayerScopedFeature<IPlayerFractionsFeature, PlayerFractionsFeature>();
        services.AddPlayerScopedFeature<IPlayerGuiFeature, PlayerGuiFeature>();
        services.AddPlayerScopedFeature<IPlayerHudFeature, PlayerHudFeature>();
        services.AddPlayerScopedFeature<IPlayerInventoryFeature, PlayerInventoryFeature>();
        services.AddPlayerScopedFeature<IPlayerNotificationsFeature, PlayerNotificationsFeature>();
        services.AddPlayerScopedFeature<IPlayerSchedulerFeature, PlayerSchedulerFeature>();
        services.AddPlayerScopedFeature<IPlayerFriendsFeature, PlayerFriendsFeature>();
        #endregion

        #region Vehicle features
        services.AddVehicleScopedFeature<IVehicleAccessFeature, VehicleAccessFeature>();
        services.AddVehicleScopedFeature<IVehiclePersistenceFeature, VehiclePersistanceFeature>();
        services.AddVehicleScopedFeature<IVehicleMileageCounterFeature, VehicleMileageCounterFeature>();
        services.AddVehicleScopedFeature<IVehicleUpgradesFeature, VehicleUpgradesFeature>();
        services.AddVehicleScopedFeature<IVehiclePartDamageFeature, VehiclePartDamageFeature>();
        services.AddVehicleScopedFeature<IVehicleEnginesFeature, VehicleEnginesFeature>();
        services.AddVehicleScopedFeature<IVehicleEventsFeature, VehicleEventsFeature>();
        services.AddVehicleScopedFeature<IVehicleFuelFeature, VehicleFuelFeature>();
        services.AddVehicleScopedFeature<IVehicleInventoryFeature, VehicleInventoryFeature>();
        #endregion

        #region Elements features
        services.AddScoped<IElementCustomDataFeature, ElementCustomDataFeature>();
        #endregion

        #region Policies
        services.AddScoped<ICommandThrottlingPolicy, CommandThrottlingPolicy>();
        #endregion

        services.AddSingleton<IVehiclesAccessService, VehiclesAccessService>();
        services.AddKeyedSingleton<IElementFactory, ElementFactory>("ElementFactory");
        services.AddSingleton(x => x.GetRequiredKeyedService<IElementFactory>("ElementFactory"));
        services.AddScoped<IScopedElementFactory, ScopedElementFactory>();
        services.AddScoped<PlayerScopedCollisionShapeBehaviour>();
        services.AddSingleton<IElementIdGenerator, RangedCollectionBasedElementIdGenerator>(x =>
            new RangedCollectionBasedElementIdGenerator(x.GetRequiredService<IElementCollection>(), IdGeneratorConstants.PlayerIdStart, IdGeneratorConstants.PlayerIdStop)
        );

        services.AddTransient<ILogger>(x => x.GetRequiredService<ILogger<MtaServer>>());
#if DEBUG
        var serverFilesProvider = new ServerFilesProvider("../../../Server");
#else
        var serverFilesProvider = new ServerFilesProvider("Server");
#endif
        services.AddSingleton<IServerFilesProvider>(serverFilesProvider);
        services.AddHostedService<MigrateDatabaseService>();
        services.AddHostedService<LoadFractionsService>();
        services.AddHostedService<SeedServerService>();
        services.AddHostedService<LoadVehicleService>();
        services.AddHostedService<BrowserGuiHostedService>();
        services.AddHostedService<AdminResourceHostedService>();
        services.AddHostedService<AFKResourceHostedService>();
        services.AddHostedService<BoneAttachResourceHostedService>();
        services.AddHostedService<ClientInterfaceResourceLogic>();
        services.AddHostedService<NametagResourceHostedService>();
        services.AddHostedService<StatisticsCounterResourceHostedService>();
        services.AddHostedService<WatermarkResourceHostedService>();
        services.AddHostedService<AdministrationHostedService>();
        services.AddHostedService<FocusableElementsHostedService>();
        services.AddHostedService<PlayerHudHostedService>();
        services.AddHostedService<VehiclesTuningHostedService>();
        services.AddHostedService<VehiclesInUseHostedService>();
        services.AddHostedService<InventoryHostedService>();
        services.AddHostedService<PlayersHostedService>();
        services.AddHostedService<ServerListHostedService>();
        services.AddHostedService<VehicleAccessControllerLogic>();
        services.AddHostedService<MapsHostedService>();
        services.AddHostedService<PlayersBindsHostedService>();
        services.AddHostedService<PlayTimeHostedService>();
        services.AddHostedService<PlayerBlipHostedService>();
        services.AddHostedService<PlayerMoneyHostedService>();
        services.AddHostedService<AFKHostedService>();
        services.AddHostedService<PlayerDailyVisitsHostedService>();
        services.AddHostedService<PlayerJoinedPipelineHostedService>();
        services.AddHostedService<ServerLifecycle>();
        services.AddHostedService<FriendsHostedService>();
        services.AddHostedService<UserLoggedInHostedService>();

        services.AddResources();

        return services;
    }
}

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

    public static readonly ActivitySource Activity = new("RealmCore.RealmServer", "1.0.0");
}