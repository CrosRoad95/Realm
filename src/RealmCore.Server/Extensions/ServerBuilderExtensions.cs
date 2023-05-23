using RealmCore.Resources.CEFBlazorGui;

namespace RealmCore.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureServer(this ServerBuilder builder, IRealmConfigurationProvider realmConfigurationProvider, ServerBuilderDefaultBehaviours? serverBuilderDefaultBehaviours = null)
    {
        var _serverConfiguration = realmConfigurationProvider.GetRequired<SlipeServer.Server.Configuration>("Server");
        builder.UseConfiguration(_serverConfiguration);
        if (serverBuilderDefaultBehaviours != null)
        {
            if (serverBuilderDefaultBehaviours != ServerBuilderDefaultBehaviours.None)
            {
                builder.AddDefaults(exceptBehaviours: serverBuilderDefaultBehaviours.Value);
            }
        }
        else
        {
            var exceptBehaviours = ServerBuilderDefaultBehaviours.DefaultChatBehaviour;
#if DEBUG
            builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour | exceptBehaviours);
#else
            builder.AddDefaults(exceptBehaviours: exceptBehaviours);
#endif
        }

        builder.ConfigureServices(services =>
        {
            // Options
            services.Configure<GameplayOptions>(realmConfigurationProvider.GetSection("Gameplay"));
            services.Configure<GrpcOptions>(realmConfigurationProvider.GetSection("Grpc"));
            services.Configure<ServerListOptions>(realmConfigurationProvider.GetSection("ServerList"));
            services.Configure<AssetsOptions>(realmConfigurationProvider.GetSection("Assets"));

            var databaseProvider = realmConfigurationProvider.Get<string>("Database:Provider");
            switch (databaseProvider)
            {
                case "MySql":
                    var connectionString = realmConfigurationProvider.Get<string>("Database:ConnectionString");
                    if (string.IsNullOrEmpty(connectionString))
                        throw new Exception("Connection string not found or empty.");
                    services.AddPersistance<MySqlDb>(db => db.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                    services.AddRealmIdentity<MySqlDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
                    break;
                case "SqlLite":
                    services.AddPersistance<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));
                    services.AddRealmIdentity<SQLiteDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
                    break;
                case "InMemory":
                    services.AddPersistance<SQLiteDb>(db => db.UseInMemoryDatabase("inMemoryDatabase"));
                    services.AddRealmIdentity<SQLiteDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
                    break;
                default:
                    throw new NotImplementedException($"Database provider '{databaseProvider}' is not supported. use 'MySql' or 'SqlLite' or 'InMemory'.");
            }

            services.AddSingleton<HelpCommand>();
            services.AddCommand<SaveCommand>();
            services.AddCommand<ReloadEntitiesCommand>();

            services.AddSingleton<IECS, ECS>();
        });

        #region Resources

        var commonOptions = new CommonResourceOptions();

        builder.AddNoClipResource(new NoClipOptions
        {
            Bind = null
        });
        builder.AddDGSResource(DGSVersion.Release_3_520);
        builder.AddGuiSystemResource(builder =>
        {
            builder.AddGuiProvider(DGSGuiProvider.Name, DGSGuiProvider.LuaCode);
            builder.SetGuiProvider(DGSGuiProvider.Name);
        }, commonOptions);
        builder.AddClientInterfaceResource(commonOptions);
        builder.AddElementOutlineResource();
        builder.AddAdminResource();
        builder.AddAFKResource();
        builder.AddStatisticsCounterResource();
        builder.AddOverlayResource();
        builder.AddAssetsResource();
        builder.AddText3dResource();
        builder.AddNametagsResource();
        builder.AddBoneAttachResource(BoneAttachVersion.Release_1_2_0);
        builder.AddWatermarkResource();
        builder.AddCEFBlazorGuiResource("E:\\Drive D\\Realm\\src\\RealmCore.BlazorCEFGui\\bin\\Release\\net7.0\\browser-wasm\\publish\\wwwroot");
        #endregion

        #region Resources Logics
        builder.AddLogic<ClientInterfaceResourceLogic>();
        builder.AddLogic<StatisticsCounterResourceLogic>();
        builder.AddLogic<AFKResourceLogic>();
        builder.AddLogic<AdminResourceLogic>();
        builder.AddLogic<OutlineResourceLogic>();
        builder.AddLogic<WatermarkResourceLogic>();
        #endregion

        // Miscellaneous logic
        builder.AddLogic<EssentialCommandsLogic>();

        return builder;
    }
}
