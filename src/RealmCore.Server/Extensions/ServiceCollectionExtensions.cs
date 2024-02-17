using Coravel;

namespace RealmCore.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static ServiceCollection AddCommand<TCommand>(this ServiceCollection services) where TCommand : class, ICommand
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

    public static ServiceCollection AddPlayerScopedService<T1, T2>(this ServiceCollection services)
        where T1 : class, IPlayerFeature
        where T2: class, T1
    {
        services.AddScoped<T1, T2>();
        services.AddScoped<IPlayerFeature>(x => x.GetRequiredService<T1>()); 
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
        services.AddScoped<ILoadService, LoadService>();
        services.AddScoped<IVehiclesService, VehiclesService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IFractionService, FractionService>();
        services.AddScoped<IRewardService, RewardService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ISpawnMarkersService, SpawnMarkersService>();
        services.AddScoped<IPlayersNotifications, PlayersNotifications>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IPlayerMoneyHistoryService, PlayerMoneyHistoryService>();
        services.AddScoped<IScopedMapsService, ScopedMapService>();
        services.AddSingleton<IBrowserGuiService, BrowserGuiService>();
        services.AddSingleton<IMapsService, MapsService>();
        services.AddSingleton<IPeriodicEventDispatcher, PeriodicEventDispatcher>();
        services.AddSingleton<IPlayersEventManager, PlayerEventManager>();
        #endregion

        #region Player services
        services.AddPlayerScopedService<IPlayerBrowserFeature, PlayerBrowserFeature>();
        services.AddPlayerScopedService<IPlayerAFKFeature, PlayerAFKFeature>();
        services.AddPlayerScopedService<IPlayerMoneyFeature, PlayerMoneyFeature>();
        services.AddPlayerScopedService<IPlayerUserFeature, PlayerUserFeature>();
        services.AddPlayerScopedService<IPlayerDailyVisitsFeature, PlayerDailyVisitsFeature>();
        services.AddPlayerScopedService<IPlayerSettingsFeature, PlayerSettingsFeature>();
        services.AddPlayerScopedService<IPlayerBansFeature, PlayerBansFeature>();
        services.AddPlayerScopedService<IPlayerUpgradesFeature, PlayerUpgradesFeature>();
        services.AddPlayerScopedService<IPlayerPlayTimeFeature, PlayerPlayTimeFeature>();
        services.AddPlayerScopedService<IPlayerLevelFeature, PlayerLevelFeature>();
        services.AddPlayerScopedService<IPlayerLicensesFeature, PlayerLicensesFeature>();
        services.AddPlayerScopedService<IPlayerStatisticsFeature, PlayerStatisticsFeature>();
        services.AddPlayerScopedService<IPlayerAchievementsFeature, PlayerAchievementsFeature>();
        services.AddPlayerScopedService<IPlayerDiscoveriesFeature, PlayerDiscoveriesFeature>();
        services.AddPlayerScopedService<IPlayerJobUpgradesFeature, PlayerJobUpgradesFeature>();
        services.AddPlayerScopedService<IPlayerJobStatisticsFeature, PlayerJobStatisticsFeature>();
        services.AddPlayerScopedService<IPlayerEventsFeature, PlayerEventsFeature>();
        services.AddPlayerScopedService<IPlayerSessionsFeature, PlayerSessionsFeature>();
        services.AddPlayerScopedService<IPlayerAdminFeature, PlayerAdminFeature>();
        services.AddPlayerScopedService<IPlayerGroupsFeature, PlayerGroupsFeature>();
        services.AddPlayerScopedService<IPlayerFractionsFeature, PlayerFractionsFeature>();
        services.AddPlayerScopedService<IPlayerGuiFeature, PlayerGuiFeature>();
        services.AddPlayerScopedService<IPlayerHudFeature, PlayerHudFeature>();
        services.AddPlayerScopedService<IPlayerInventoryFeature, PlayerInventoryFeature>();
        services.AddPlayerScopedService<IPlayerNotificationsFeature, PlayerNotificationsFeature>();
        #endregion

        #region Vehicle services
        services.AddScoped<IVehicleAccessFeature, VehicleAccessFeature>();
        services.AddScoped<IVehiclePersistenceFeature, VehiclePersistanceFeature>();
        services.AddScoped<IVehicleMileageCounterFeature, VehicleMileageCounterFeature>();
        services.AddScoped<IVehiclesAccessService, VehiclesAccessService>();
        services.AddScoped<IVehicleUpgradesFeature, VehicleUpgradesFeature>();
        services.AddScoped<IVehiclePartDamageFeature, VehiclePartDamageFeature>();
        services.AddScoped<IVehicleEnginesFeature, VehicleEnginesFeature>();
        services.AddScoped<IVehicleEventsFeature, VehicleEventsFeature>();
        services.AddScoped<IVehicleFuelFeature, VehicleFuelFeature>();
        services.AddScoped<IVehicleInventoryFeature, VehicleInventoryFeature>();
        #endregion

        #region Policies
        services.AddScoped<ICommandThrottlingPolicy, CommandThrottlingPolicy>();
        #endregion

        services.AddKeyedScoped<IElementFactory, ElementFactory>("ElementFactory");
        services.AddScoped<IElementFactory>(x => x.GetRequiredKeyedService<IElementFactory>("ElementFactory"));
        services.AddScoped<IScopedElementFactory, ScopedElementFactory>();
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

        #region 3-rd party
        services.AddScheduler(); // Coravel
        #endregion

        return services;
    }
}
