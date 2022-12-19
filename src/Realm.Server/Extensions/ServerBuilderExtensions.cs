namespace Realm.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureServer(this ServerBuilder builder, RealmConfigurationProvider realmConfigurationProvider)
    {
        var _serverConfiguration = realmConfigurationProvider.GetRequired<SlipeServer.Server.Configuration>("Server");
        builder.UseConfiguration(_serverConfiguration);
#if DEBUG
        builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
#else
        builder.AddDefaults();
#endif

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(realmConfigurationProvider);

            services.AddPersistance<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));
            services.AddRealmIdentity<SQLiteDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));

            services.AddSingleton<HelpCommand>();
            services.AddSingleton<ICommand, TestCommand>();
        });

        // Resource
        var commonOptions = new CommonResourceOptions();

        builder.AddNoClipResource();
        builder.AddDGSResource(DGSVersion.Release_3_518);
        builder.AddAgnosticGuiSystemResource(builder =>
        {
            builder.AddGuiProvider(CeGuiGuiProvider.Name, CeGuiGuiProvider.LuaCode);
            builder.AddGuiProvider(DGSGuiProvider.Name, DGSGuiProvider.LuaCode);
        }, commonOptions);
        builder.AddLuaInteropResource(commonOptions);
        builder.AddElementOutlineResource();
        builder.AddAdminToolsResource();
        builder.AddAFKResource();
        builder.AddStatisticsCounterResource();
        builder.AddOverlayResource();
        builder.AddAssets();

        // Resources logics
        builder.AddLogic<LuaInteropLogic>();

        // Miscellaneous logic
        builder.AddLogic<CommandsLogic>();

        return builder;
    }
}
