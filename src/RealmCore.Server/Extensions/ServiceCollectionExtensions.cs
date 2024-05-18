using RealmCore.Server.Modules.Players.Settings;

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
    
    public static IServiceCollection AddServerLoader<TServerLoader>(this IServiceCollection services) where TServerLoader : class, IServerLoader
    {
        services.AddTransient<IServerLoader, TServerLoader>();
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

    public static IServiceCollection ConfigureRealmServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Options
        services.Configure<GameplayOptions>(configuration.GetSection("Gameplay"));
        services.Configure<ServerListOptions>(configuration.GetSection("ServerList"));
        services.Configure<AssetsOptions>(configuration.GetSection("Assets"));
        services.Configure<GuiBrowserOptions>(configuration.GetSection("GuiBrowser"));
        services.Configure<BrowserOptions>(configuration.GetSection("Browser"));

        var connectionString = configuration.GetValue<string>("Database:ConnectionString");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddPersistence<MySqlDb>(db => db.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
            var identityConfiguration = configuration.GetSection("Identity").Get<IdentityConfiguration>();
            if (identityConfiguration == null)
                throw new Exception("Identity configuration is null");
            services.AddRealmIdentity<MySqlDb>(identityConfiguration);
        }

        services.AddResources();
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
        #endregion

        #region Services
        services.AddSingleton<IUsersService, UsersService>();
        services.AddScoped<IElementSaveService, ElementSaveService>();
        services.AddScoped<IVehicleLoader, VehicleLoader>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddSingleton<IGroupsService, GroupsService>();
        services.AddScoped<IFractionsService, FractionService>();
        services.AddSingleton<IRewardsService, RewardsService>();
        services.AddScoped<IFeedbackService, PlayerFeedbackService>();
        services.AddSingleton<ISpawnMarkersService, SpawnMarkersService>();
        services.AddScoped<IPlayersNotifications, PlayersNotifications>();
        services.AddScoped<INewsService, PlayerNewsService>();
        services.AddSingleton<IMoneyHistoryService, MoneyHistoryService>();
        services.AddScoped<IPlayerMapsService, PlayerMapsService>();
        services.AddScoped<IElementSearchService, ElementSearchService>();
        services.AddScoped<IPlayerUserService, PlayersUsersService>();
        services.AddSingleton<IBrowserGuiService, BrowserGuiService>();
        services.AddSingleton<IMapsService, MapsService>();
        services.AddSingleton<PlayersEventManager>();
        services.AddSingleton<ISchedulerService, SchedulerService>();
        services.AddSingleton<IVehiclesService, VehiclesService>();
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
        services.AddSingleton<IElementFactory>(x => x.GetRequiredKeyedService<IElementFactory>("ElementFactory"));
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
        services.AddServerLoader<MigrateDatabaseServerLoader>();
        services.AddServerLoader<LoadFractionsServerLoader>();
        services.AddServerLoader<SeederServerLoader>();
        services.AddServerLoader<VehicleServerLoader>();

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
        services.AddHostedService<GuiHostedService>();
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

        return services;
    }
}

internal sealed class ServerLifecycle : IHostedService
{
    private readonly ILogger<ServerLifecycle> _logger;
    private readonly IEnumerable<IServerLoader> _serverLoaders;
    private readonly IOptions<GameplayOptions> _gameplayOptions;
    private readonly RealmCommandService _realmCommandService;
    private readonly IElementCollection _elementCollection;

    public ServerLifecycle(ILogger<ServerLifecycle> logger, IEnumerable<IServerLoader> serverLoaders, IOptions<GameplayOptions> _gameplayOptions, RealmCommandService realmCommandService, CollisionShapeBehaviour collisionShapeBehaviour, IElementCollection elementCollection)
    {
        _logger = logger;
        _serverLoaders = serverLoaders;
        this._gameplayOptions = _gameplayOptions;
        _realmCommandService = realmCommandService;
        _elementCollection = elementCollection;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var activity = Activity.StartActivity(nameof(StartAsync));

        {
            using var serverLoadersActivity = Activity.StartActivity("Loading server");
            bool anyFailed = false;
            foreach (var item in _serverLoaders)
            {
                using var serverLoaderActivity = Activity.StartActivity($"Loading {item}");
                try
                {
                    await item.Load();
                }
                catch (Exception ex)
                {
                    anyFailed = true;
                    serverLoaderActivity?.SetStatus(ActivityStatusCode.Error);
                    _logger.LogError(ex, "Failed to load server loader.");
                }
            }
            if (anyFailed)
            {
                throw new Exception("Server failed to load.");
            }
        }

        CultureInfo.CurrentCulture = _gameplayOptions.Value.Culture;
        CultureInfo.CurrentUICulture = _gameplayOptions.Value.Culture;

        _logger.LogInformation("Server started.");
        _logger.LogInformation("Found resources: {resourcesCount}", RealmResourceServer._resourceCounter);
        _logger.LogInformation("Created commands: {commandsCount}", _realmCommandService.Count);
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
            finally
            {
                element.Destroy();
            }
        }

        _logger.LogInformation("Server stopped, saved: {savedElementsCount} elements.", i);
        await Task.Delay(500, CancellationToken.None);
    }

    public static readonly ActivitySource Activity = new("RealmCore.RealmServer", "1.0.0");
}