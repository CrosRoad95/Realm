namespace RealmCore.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureServer(this ServerBuilder serverBuilder, IRealmConfigurationProvider realmConfigurationProvider, ServerBuilderDefaultBehaviours? serverBuilderDefaultBehaviours = null)
    {
        var _serverConfiguration = realmConfigurationProvider.GetRequired<SlipeServer.Server.Configuration>("Server");
        serverBuilder.UseConfiguration(_serverConfiguration);
        if (serverBuilderDefaultBehaviours != null)
        {
            if (serverBuilderDefaultBehaviours != ServerBuilderDefaultBehaviours.None)
            {
                serverBuilder.AddDefaults(exceptBehaviours: serverBuilderDefaultBehaviours.Value);
            }
        }
        else
        {
            serverBuilder.AddResourceServer<RealmResourceServer>();
            var exceptBehaviours = ServerBuilderDefaultBehaviours.DefaultChatBehaviour | ServerBuilderDefaultBehaviours.NicknameChangeBehaviour;
#if DEBUG
            serverBuilder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour | exceptBehaviours);
#else
            serverBuilder.AddDefaults(exceptBehaviours: exceptBehaviours);
#endif
        }

        serverBuilder.ConfigureServices(services =>
        {
            // Options
            services.Configure<GameplayOptions>(realmConfigurationProvider.GetSection("Gameplay"));
            services.Configure<ServerListOptions>(realmConfigurationProvider.GetSection("ServerList"));
            services.Configure<AssetsOptions>(realmConfigurationProvider.GetSection("Assets"));
            services.Configure<GuiBrowserOptions>(realmConfigurationProvider.GetSection("GuiBrowser"));
            services.Configure<BrowserOptions>(realmConfigurationProvider.GetSection("Browser"));

            var connectionString = realmConfigurationProvider.Get<string>("Database:ConnectionString");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddPersistence<MySqlDb>(db => db.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                services.AddRealmIdentity<MySqlDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
            }

            services.AddSingleton<HelpCommand>();
            services.AddCommand<SaveCommand>(); 
            services.AddCommand<ServerInfoCommand>(); 
            services.AddCommand<ReloadElementsCommand>();
        });

        #region Resources

        var commonOptions = new CommonResourceOptions();

        serverBuilder.AddNoClipResource(new NoClipOptions
        {
            Bind = null
        });

        serverBuilder.AddClientInterfaceResource(commonOptions);
        serverBuilder.AddElementOutlineResource();
        serverBuilder.AddAdminResource();
        serverBuilder.AddAFKResource();
        serverBuilder.AddMapNamesResource();
        serverBuilder.AddStatisticsCounterResource();
        serverBuilder.AddOverlayResource();
        serverBuilder.AddAssetsResource();
        serverBuilder.AddText3dResource();
        serverBuilder.AddNametagsResource();
        serverBuilder.AddBoneAttachResource(BoneAttachVersion.Release_1_2_0);
        serverBuilder.AddWatermarkResource();
        serverBuilder.AddScoreboard();
        #endregion

        #region Logics
        serverBuilder.AddLogic<SchedulerLogic>();
        serverBuilder.AddLogic<BrowserGuiLogic>();
        serverBuilder.AddLogic<AdminResourceLogic>();
        serverBuilder.AddLogic<AFKResourceLogic>();
        serverBuilder.AddLogic<BoneAttachResourceLogic>();
        serverBuilder.AddLogic<ClientInterfaceResourceLogic>();
        serverBuilder.AddLogic<NametagResourceLogic>();
        serverBuilder.AddLogic<StatisticsCounterResourceLogic>();
        serverBuilder.AddLogic<WatermarkResourceLogic>();
        serverBuilder.AddLogic<AdministrationLogic>();
        serverBuilder.AddLogic<FocusableElementsLogic>();
        serverBuilder.AddLogic<PlayerHudLogic>();
        serverBuilder.AddLogic<VehiclesTuningLogic>();
        serverBuilder.AddLogic<PickupLogic>();
        serverBuilder.AddLogic<CollisionShapeLogic>();
        serverBuilder.AddLogic<PeristantVehicleLogic>();
        serverBuilder.AddLogic<InventoryLogic>();
        serverBuilder.AddLogic<PlayersLogic>();
        serverBuilder.AddLogic<GuiLogic>();
        serverBuilder.AddLogic<ServerLogic>();
        serverBuilder.AddLogic<VehicleAccessControllerLogic>();
        serverBuilder.AddLogic<MapsLogic>();
        serverBuilder.AddLogic<PlayersBindsLogic>();
        serverBuilder.AddLogic<PlayTimeLogic>();
        serverBuilder.AddLogic<PlayerBlipLogic>();
        serverBuilder.AddLogic<PlayerMoneyLogic>();
        serverBuilder.AddLogic<AFKLogic>();
        serverBuilder.AddLogic<PlayerDailyVisitsLogic>();
        #endregion

        #region Behaviours
        serverBuilder.AddBehaviour<ScopedCollisionShapeBehaviour>();
        #endregion

        return serverBuilder;
    }

    public static ServerBuilder WithGuiSystem(this ServerBuilder serverBuilder)
    {
        serverBuilder.AddScopedLogic<DxGuiSystemServiceLogic>();
        serverBuilder.AddScopedLogic<ReactiveDxGuiLogic>();
        return serverBuilder;
    }

}
