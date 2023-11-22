using RealmCore.Server.Factories;
using RealmCore.Server.Services.Players;
using RealmCore.Server.Services.Vehicles;

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

    internal static ServiceCollection AddPlayerScopedService<T1, T2>(this ServiceCollection services)
        where T1 : class, IPlayerService
        where T2: class, T1
    {
        services.AddScoped<T1, T2>();
        services.AddScoped<IPlayerService>(x => x.GetRequiredService<T1>()); 
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
        #endregion

        #region Registries
        services.AddSingleton<MapsRegistry>();
        services.AddSingleton<ItemsRegistry>();
        services.AddSingleton<VehicleUpgradeRegistry>();
        services.AddSingleton<LevelsRegistry>();
        services.AddSingleton<VehicleEnginesRegistry>();
        #endregion

        #region Security
        services.AddSingleton<IActiveUsers, ActiveUsers>();
        services.AddSingleton<IActiveVehicles, ActiveVehicles>();
        #endregion

        #region Services
        services.AddSingleton<IUsersService, UsersService>();
        services.AddScoped<ISaveService, SaveService>();
        services.AddScoped<ILoadService, LoadService>();
        services.AddScoped<IVehiclesService, VehiclesService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IFractionService, FractionService>();
        services.AddScoped<IBanService, BanService>();
        services.AddScoped<IRewardService, RewardService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<ISpawnMarkersService, SpawnMarkersService>();
        services.AddScoped<IUsersNotificationsService, UsersNotificationsService>();
        services.AddScoped<IVehicleAccessService, VehicleAccessService>();
        services.AddScoped<IVehiclePersistanceService, VehiclePersistanceService>();
        services.AddScoped<IVehicleMileageService, VehicleMileageService>();
        services.AddScoped<INewsService, NewsService>();
        services.AddScoped<IUserMoneyHistoryService, UserMoneyHistoryService>();
        services.AddScoped<IScopedMapsService, ScopedMapService>();
        services.AddSingleton<IBrowserGuiService, BrowserGuiService>();
        services.AddSingleton<IMapsService, MapsService>();
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<IPlayersService, PlayersService>();
        #endregion

        #region Player services
        services.AddPlayerScopedService<IPlayerBrowserService, PlayerBrowserService>();
        services.AddPlayerScopedService<IPlayerAFKService, PlayerAFKService>();
        services.AddPlayerScopedService<IPlayerMoneyService, PlayerMoneyService>();
        services.AddPlayerScopedService<IPlayerUserService, PlayerUserService>();
        services.AddPlayerScopedService<IPlayerDailyVisitsService, PlayerDailyVisitsService>();
        services.AddPlayerScopedService<IPlayerSettingsService, PlayerSettingsService>();
        services.AddPlayerScopedService<IPlayerBansService, PlayerBansService>();
        services.AddPlayerScopedService<IPlayerUpgradeService, PlayerUpgradeService>();
        services.AddPlayerScopedService<IPlayerPlayTimeService, PlayerPlayTimeService>();
        services.AddPlayerScopedService<IPlayerLevelService, PlayerLevelService>();
        services.AddPlayerScopedService<IPlayerLicensesService, PlayerLicensesService>();
        services.AddPlayerScopedService<IPlayerStatisticsService, PlayerStatisticsService>();
        services.AddPlayerScopedService<IPlayerAchievementsService, PlayerAchievementsService>();
        services.AddPlayerScopedService<IPlayerDiscoveriesService, PlayerDiscoveriesService>();
        services.AddPlayerScopedService<IPlayerJobUpgradesService, PlayerJobUpgradesService>();
        services.AddPlayerScopedService<IPlayerJobStatisticsService, PlayerJobStatisticsService>();
        services.AddPlayerScopedService<IPlayerEventsService, PlayerEventsService>();
        services.AddPlayerScopedService<IPlayerSessionsService, PlayerSessionsService>();
        #endregion

        #region Policies
        services.AddScoped<ICommandThrottlingPolicy, CommandThrottlingPolicy>();
        #endregion

        services.AddScoped<IElementFactory, ElementFactory>();
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

        return services;
    }

}
