using RealmCore.Server.Modules.Players.Settings;

namespace RealmCore.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddCommand<TCommand>(this ServiceCollection services) where TCommand : class, IInGameCommand
    {
        services.AddSingleton(new CommandTypeWrapper(typeof(TCommand)));
        services.AddTransient<TCommand>();
        return services;
    }

    public static ServiceCollection AddSeederProvider<TSeederProvider>(this ServiceCollection services) where TSeederProvider : class, ISeederProvider
    {
        services.AddSingleton<ISeederProvider, TSeederProvider>();
        return services;
    }

    public static ServiceCollection AddAsyncSeederProvider<TSeederProvider>(this ServiceCollection services) where TSeederProvider : class, IAsyncSeederProvider
    {
        services.AddSingleton<IAsyncSeederProvider, TSeederProvider>();
        return services;
    }

    public static ServiceCollection AddInGameCommand<TInGameCommand>(this ServiceCollection services) where TInGameCommand : class, IInGameCommand
    {
        services.AddTransient<IInGameCommand>(x => x.GetRequiredService<TInGameCommand>());
        services.AddTransient<TInGameCommand>();
        return services;
    }
    
    public static ServiceCollection AddServerLoader<TServerLoader>(this ServiceCollection services) where TServerLoader : class, IServerLoader
    {
        services.AddTransient<IServerLoader, TServerLoader>();
        return services;
    }

    public static ServiceCollection AddPlayerScopedFeature<T1, T2>(this ServiceCollection services)
        where T1 : class, IPlayerFeature
        where T2: class, T1
    {
        services.AddScoped<T1, T2>();
        services.AddScoped<IPlayerFeature>(x => x.GetRequiredService<T1>()); 
        return services;
    }
    
    public static ServiceCollection AddVehicleScopedFeature<T1, T2>(this ServiceCollection services)
        where T1 : class, IVehicleFeature
        where T2: class, T1
    {
        services.AddScoped<T1, T2>();
        services.AddScoped<IVehicleFeature>(x => x.GetRequiredService<T1>()); 
        return services;
    }
    
    public static ServiceCollection AddPlayerJoinedPipeline<T>(this ServiceCollection services) where T : class, IPlayerJoinedPipeline
    {
        services.AddTransient<IPlayerJoinedPipeline, T>();
        return services;
    }

    internal static ServiceCollection ConfigureRealmServices(this ServiceCollection services)
    {
        var consoleLogger = services.Where(x => x.ImplementationType == typeof(ConsoleLoggerProvider)).FirstOrDefault();
        if (consoleLogger != null)
            services.Remove(consoleLogger);

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
        services.AddSingleton<IPlayersService, PlayersService>();
        services.AddSingleton<IUsersService, UsersService>();
        services.AddScoped<ISaveService, SaveService>();
        services.AddScoped<IVehicleLoader, VehicleLoader>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IFractionService, FractionService>();
        services.AddScoped<IRewardService, RewardService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ISpawnMarkersService, SpawnMarkersService>();
        services.AddScoped<IPlayersNotifications, PlayersNotifications>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IPlayerMoneyHistoryService, PlayerMoneyHistoryService>();
        services.AddScoped<IScopedMapsService, ScopedMapService>();
        services.AddScoped<IElementSearchService, ElementSearchService>();
        services.AddScoped<IPlayerUserService, PlayerUserService>();
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

        return services;
    }
}
