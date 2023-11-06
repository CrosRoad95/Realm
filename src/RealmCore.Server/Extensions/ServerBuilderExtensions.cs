using RealmCore.Resources.Browser;
using RealmCore.Server.Behaviours;
using RealmCore.Server.Logic.Components;
using RealmCore.Server.Logic.Elements;
using RealmCore.SQLite;
using SlipeServer.Resources.Scoreboard;

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
            var exceptBehaviours = ServerBuilderDefaultBehaviours.DefaultChatBehaviour;
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
            services.Configure<BrowserOptions>(realmConfigurationProvider.GetSection("Browser"));

            var databaseProvider = realmConfigurationProvider.Get<string>("Database:Provider");
            switch (databaseProvider)
            {
                case "MySql":
                    var connectionString = realmConfigurationProvider.Get<string>("Database:ConnectionString");
                    if (string.IsNullOrEmpty(connectionString))
                        throw new Exception("Connection string not found or empty.");
                    services.AddPersistence<MySqlDb>(db => db.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                    services.AddRealmIdentity<MySqlDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
                    break;
                case "SqlLite":
                    var sqlLiteFileName = realmConfigurationProvider.Get<string>("Database:SqlLiteFileName");
                    services.AddPersistence<SQLiteDb>(db => db.UseSqlite($"Filename=./{sqlLiteFileName ?? "server"}.db"));
                    services.AddRealmIdentity<SQLiteDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
                    break;
                case "InMemory":
                    services.AddPersistence<SQLiteDb>(db => db.UseInMemoryDatabase("inMemoryDatabase"));
                    services.AddRealmIdentity<SQLiteDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
                    break;
                default:
                    throw new NotImplementedException($"Database provider '{databaseProvider}' is not supported. use 'MySql' or 'SqlLite' or 'InMemory'.");
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
        serverBuilder.AddStatisticsCounterResource();
        serverBuilder.AddOverlayResource();
        serverBuilder.AddAssetsResource();
        serverBuilder.AddText3dResource();
        serverBuilder.AddNametagsResource();
        serverBuilder.AddBoneAttachResource(BoneAttachVersion.Release_1_2_0);
        serverBuilder.AddWatermarkResource();
        serverBuilder.AddScoreboard();
        #endregion

        #region Resources Logics
        serverBuilder.AddScopedLogic<BrowserGuiComponentLogic>();
        serverBuilder.AddScopedLogic<AdminResourceLogic>();
        serverBuilder.AddScopedLogic<AFKResourceLogic>();
        serverBuilder.AddScopedLogic<BoneAttachResourceLogic>();
        serverBuilder.AddScopedLogic<ClientInterfaceResourceLogic>();
        serverBuilder.AddScopedLogic<NametagResourceLogic>();
        serverBuilder.AddScopedLogic<OutlineResourceLogic>();
        serverBuilder.AddScopedLogic<StatisticsCounterResourceLogic>();
        serverBuilder.AddScopedLogic<Text3dComponentLogic>();
        serverBuilder.AddScopedLogic<WatermarkResourceLogic>();
        serverBuilder.AddScopedLogic<DailyVisitsCounterComponentLogic>();
        serverBuilder.AddScopedLogic<AdminComponentLogic>();
        serverBuilder.AddLogic<FocusableComponentLogic>();
        serverBuilder.AddScopedLogic<StatefulHudComponentLogic>();
        serverBuilder.AddScopedLogic<JobSessionComponentLogic>();
        serverBuilder.AddScopedLogic<InteractionComponentLogic>();
        serverBuilder.AddScopedLogic<Hud3dComponentBaseLogic>();
        serverBuilder.AddScopedLogic<LiftableWorldObjectComponentLogic>();
        serverBuilder.AddLogic<VehicleUpgradesComponentLogic>();
        serverBuilder.AddLogic<UpdateCallbackLogic>();
        serverBuilder.AddLogic<PickupLogic>();
        serverBuilder.AddLogic<CollisionShapeLogic>();
        serverBuilder.AddScopedLogic<PrivateVehicleComponentLogic>();
        serverBuilder.AddScopedLogic<InventoryComponentLogic>();
        serverBuilder.AddScopedLogic<PlayersLogic>();
        serverBuilder.AddScopedLogic<GuiLogic>();
        serverBuilder.AddScopedLogic<ServerLogic>();
        serverBuilder.AddScopedLogic<BanLogic>();
        serverBuilder.AddScopedLogic<VehicleEnginesRegistryLogic>();
        serverBuilder.AddScopedLogic<VehicleAccessControllerComponentLogic>();
        serverBuilder.AddScopedLogic<MapsLogic>();
        #endregion

        #region Behaviours
        serverBuilder.AddBehaviour<ScopedCollisionShapeBehaviour>();
        #endregion

        return serverBuilder;
    }

    public static ServerBuilder WithGuiSystem(this ServerBuilder serverBuilder)
    {
        serverBuilder.AddScopedLogic<DxGuiSystemServiceLogic>();
        serverBuilder.AddScopedLogic<StatefulGuiComponentBaseLogic>();
        return serverBuilder;
    }

}
