using RealmCore.Resources.Browser;
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
                    services.AddPersistence<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));
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
            services.AddCommand<ReloadEntitiesCommand>();

            services.AddSingleton<IEntityEngine, EntityEngine>();
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
        serverBuilder.AddLogic<BrowserGuiPageComponentLogic>();
        serverBuilder.AddLogic<AdminResourceLogic>();
        serverBuilder.AddLogic<AFKResourceLogic>();
        serverBuilder.AddLogic<BoneAttachResourceLogic>();
        serverBuilder.AddLogic<BrowserResourceLogic>();
        serverBuilder.AddLogic<ClientInterfaceResourceLogic>();
        serverBuilder.AddLogic<NametagResourceLogic>();
        serverBuilder.AddLogic<OutlineResourceLogic>();
        serverBuilder.AddLogic<StatisticsCounterResourceLogic>();
        serverBuilder.AddLogic<Text3dComponentLogic>();
        serverBuilder.AddLogic<WatermarkResourceLogic>();
        serverBuilder.AddLogic<DailyVisitsCounterComponentLogic>();
        serverBuilder.AddLogic<AdminComponentLogic>();
        serverBuilder.AddLogic<FocusableComponentLogic>();
        serverBuilder.AddLogic<CollisionShapeElementComponentLogic>();
        serverBuilder.AddLogic<StatefulHudComponentLogic>();
        serverBuilder.AddLogic<JobSessionComponentLogic>();
        serverBuilder.AddLogic<InteractionComponentLogic>();
        serverBuilder.AddLogic<Hud3dComponentBaseLogic>();
        serverBuilder.AddLogic<LiftableWorldObjectComponentLogic>();
        serverBuilder.AddLogic<VehicleUpgradesComponentLogic>();
        serverBuilder.AddLogic<UpdateCallbackLogic>();
        serverBuilder.AddLogic<PickupElementComponentLogic>();
        serverBuilder.AddLogic<UserComponentLogic>();
        #endregion

        return serverBuilder;
    }

    public static ServerBuilder WithGuiSystem(this ServerBuilder serverBuilder)
    {
        serverBuilder.AddLogic<DxGuiSystemServiceLogic>();
        serverBuilder.AddLogic<StatefulGuiComponentBaseLogic>();
        return serverBuilder;
    }

}
