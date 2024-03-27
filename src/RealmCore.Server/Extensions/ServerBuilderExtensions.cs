using Microsoft.Extensions.Configuration;

namespace RealmCore.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureServer(this ServerBuilder serverBuilder, IConfiguration configuration, ServerBuilderDefaultBehaviours? serverBuilderDefaultBehaviours = null)
    {
        var _serverConfiguration = configuration.GetRequired<SlipeServer.Server.Configuration>("Server");
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
            services.Configure<GameplayOptions>(configuration.GetSection("Gameplay"));
            services.Configure<ServerListOptions>(configuration.GetSection("ServerList"));
            services.Configure<AssetsOptions>(configuration.GetSection("Assets"));
            services.Configure<GuiBrowserOptions>(configuration.GetSection("GuiBrowser"));
            services.Configure<BrowserOptions>(configuration.GetSection("Browser"));

            var connectionString = configuration.Get<string>("Database:ConnectionString");
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddPersistence<MySqlDb>(db => db.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                services.AddRealmIdentity<MySqlDb>(configuration.GetRequired<IdentityConfiguration>("Identity"));
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
        serverBuilder.AddLogic<PlayerJoinedPipeline>();
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
