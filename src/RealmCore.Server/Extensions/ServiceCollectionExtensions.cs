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

    internal static ServiceCollection ConfigureRealmServices(this ServiceCollection services)
    {
        var consoleLogger = services.Where(x => x.ImplementationType == typeof(ConsoleLoggerProvider)).FirstOrDefault();
        if (consoleLogger != null)
            services.Remove(consoleLogger);

        #region Common
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<IAssetEncryptionProvider, AssetEncryptionProvider>();
        services.AddSingleton<SeederServerBuilder>();
        services.AddSingleton<RealmCommandService>();
        #endregion

        #region Registries
        services.AddSingleton<ItemsRegistry>();
        services.AddSingleton<VehicleUpgradeRegistry>();
        services.AddSingleton<LevelsRegistry>();
        services.AddSingleton<VehicleEnginesRegistry>();
        #endregion

        #region Security
        services.AddSingleton<IActiveUsers, ActiveUsers>();
        services.AddSingleton<IActiveVehicles, ActiveVehicles>();
        #endregion

        #region Common
        services.AddTransient<IUsersService, UsersService>();
        services.AddTransient<ISaveService, SaveService>();
        services.AddTransient<ILoadService, LoadService>();
        services.AddTransient<IVehiclesService, VehiclesService>();
        services.AddTransient<IGroupService, GroupService>();
        services.AddTransient<IFractionService, FractionService>();
        services.AddTransient<IBanService, BanService>();
        services.AddTransient<IJobService, JobService>();
        services.AddTransient<IRewardService, RewardService>();
        services.AddTransient<IFeedbackService, FeedbackService>();
        services.AddSingleton<IMapsService, MapsService>();
        services.AddSingleton<ISpawnMarkersService, SpawnMarkersService>();
        services.AddSingleton<IUsersNotificationsService, UsersNotificationsService>();
        services.AddSingleton<IVehicleAccessService, VehicleAccessService>();
        services.AddSingleton<IBrowserGuiService, BrowserGuiService>();
        #endregion

        #region Policies
        services.AddSingleton<IPolicyDrivenCommandExecutor, PolicyDrivenCommandExecutor>();
        #endregion

        services.AddTransient<IEntityFactory, EntityFactory>();
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
        services.AddSingleton<IRealmServer>(x => (RealmServer)x.GetRequiredService<MtaServer>());

        return services;
    }

}
