using RealmCore.Server.Modules.Businesses;
using RealmCore.Server.Modules.Players.Avatars;
using RealmCore.Server.Modules.Players.Discord;
using RealmCore.Server.Modules.Players.Integrations;
using RealmCore.Server.Modules.Players.Rewards;
using RealmCore.Server.Modules.Players.Secrets;
using RealmCore.Server.Modules.TimeBaseOperations;
using RealmCore.Server.Modules.Vehicles.Settings;
using RealmCore.Server.Modules.World.WorldNodes;

namespace RealmCore.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlayerScopedFeature<T1>(this IServiceCollection services)
        where T1 : class, IPlayerFeature
    {
        services.AddScoped<T1>();
        services.AddTransient<IPlayerFeature>(x => x.GetRequiredService<T1>()); 
        return services;
    }
    
    public static IServiceCollection AddVehicleScopedFeature<T1>(this IServiceCollection services)
        where T1 : class, IVehicleFeature
    {
        services.AddScoped<T1>();
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
        services.AddSingleton<UsersInUse>();
        services.AddSingleton<VehiclesInUse>();
        services.AddSingleton<AntiCheat>();
        services.AddHostedService<AntiCheatService>();
        #endregion

        #region Services
        services.AddScoped<ElementSaveService>();
        services.AddScoped<VehiclesLoader>();
        services.AddScoped<VehicleService>();
        services.AddScoped<PlayerSearchService>();
        services.AddSingleton<UsersService>();
        services.AddSingleton<UsersService>();
        services.AddSingleton<GroupsService>();
        services.AddSingleton<TimeBaseOperationsService>();
        services.AddSingleton<BusinessesService>();
        services.AddSingleton<BansService>();
        services.AddSingleton<RewardsService>();
        services.AddSingleton<FeedbackService>();
        services.AddSingleton<SpawnMarkersService>();
        services.AddSingleton<NewsService>();
        services.AddSingleton<BrowserGuiService>();
        services.AddSingleton<MapsService>();
        services.AddSingleton<PlayersEventManager>();
        services.AddSingleton<ISchedulerService, SchedulerService>();
        services.AddSingleton<VehiclesService>();
        services.AddSingleton<CollisionShapeBehaviour>();
        services.AddSingleton<ScopedCollisionShapeBehaviour>();
        services.AddSingleton<FriendsService>();
        services.AddSingleton<IRealmResourcesProvider, RealmResourcesProvider>();
        services.AddSingleton<IResourceServer>(x => new RealmResourceServer(x.GetRequiredService<BasicHttpServer>(), x.GetRequiredService<IRealmResourcesProvider>()));
        services.AddSingleton<NotificationsService>();
        services.AddSingleton<WorldNodesService>();
        services.AddSingleton<MapLoader>();
        services.AddSingleton<VehiclesAccessService>();
        services.AddSingleton<DataEventsService>();
        services.AddSingleton<GroupsManager>();
        services.AddSingleton<WorldHudService>();
        #endregion

        #region Player features
        services.AddPlayerScopedFeature<PlayerBrowserFeature>();
        services.AddPlayerScopedFeature<PlayerAFKFeature>();
        services.AddPlayerScopedFeature<PlayerMoneyFeature>();
        services.AddPlayerScopedFeature<PlayerUserFeature>();
        services.AddPlayerScopedFeature<PlayerDailyVisitsFeature>();
        services.AddPlayerScopedFeature<PlayerSettingsFeature>();
        services.AddPlayerScopedFeature<PlayerBansFeature>();
        services.AddPlayerScopedFeature<PlayerUpgradesFeature>();
        services.AddPlayerScopedFeature<PlayerPlayTimeFeature>();
        services.AddPlayerScopedFeature<PlayerLevelFeature>();
        services.AddPlayerScopedFeature<PlayerLicensesFeature>();
        services.AddPlayerScopedFeature<PlayerStatisticsFeature>();
        services.AddPlayerScopedFeature<PlayerAchievementsFeature>();
        services.AddPlayerScopedFeature<PlayerDiscoveriesFeature>();
        services.AddPlayerScopedFeature<PlayerJobUpgradesFeature>();
        services.AddPlayerScopedFeature<PlayerJobStatisticsFeature>();
        services.AddPlayerScopedFeature<PlayerEventsFeature>();
        services.AddPlayerScopedFeature<PlayerSessionsFeature>();
        services.AddPlayerScopedFeature<PlayerAdminFeature>();
        services.AddPlayerScopedFeature<PlayerGroupsFeature>();
        services.AddPlayerScopedFeature<PlayerGuiFeature>();
        services.AddPlayerScopedFeature<PlayerHudFeature>();
        services.AddPlayerScopedFeature<PlayerInventoryFeature>();
        services.AddPlayerScopedFeature<PlayerNotificationsFeature>();
        services.AddPlayerScopedFeature<PlayerSchedulerFeature>();
        services.AddPlayerScopedFeature<PlayerFriendsFeature>();
        services.AddPlayerScopedFeature<PlayerDailyTasksFeature>();
        services.AddPlayerScopedFeature<PlayerIntegrationsFeature>();
        services.AddPlayerScopedFeature<PlayerBoostsFeature>();
        services.AddPlayerScopedFeature<PlayerSecretsFeature>();
        services.AddPlayerScopedFeature<DiscordRichPresenceFeature>();
        services.AddPlayerScopedFeature<PlayerAvatarFeature>();
        #endregion

        #region Vehicle features
        services.AddVehicleScopedFeature<VehicleAccessFeature>();
        services.AddVehicleScopedFeature<VehiclePersistenceFeature>();
        services.AddVehicleScopedFeature<VehicleMileageCounterFeature>();
        services.AddVehicleScopedFeature<VehicleUpgradesFeature>();
        services.AddVehicleScopedFeature<VehiclePartDamageFeature>();
        services.AddVehicleScopedFeature<VehicleEnginesFeature>();
        services.AddVehicleScopedFeature<VehicleEventsFeature>();
        services.AddVehicleScopedFeature<VehicleFuelFeature>();
        services.AddVehicleScopedFeature<VehicleInventoryFeature>();
        services.AddVehicleScopedFeature<VehicleSettingsFeature>();
        #endregion

        #region Elements features
        services.AddScoped<IElementCustomDataFeature, ElementCustomDataFeature>();
        #endregion

        #region Policies
        services.AddScoped<ICommandThrottlingPolicy, CommandThrottlingPolicy>();
        #endregion

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
        services.AddHostedService<SeedServerService>();
        services.AddHostedService<LoadVehiclesService>();
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
        services.AddHostedService<WorldNodesHostedService>();
        services.AddHostedService<GroupsLogic>();

        services.AddResources();

        return services;
    }
}
