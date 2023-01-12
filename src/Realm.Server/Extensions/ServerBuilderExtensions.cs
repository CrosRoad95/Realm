using Realm.Persistance.MySql;
using SlipeServer.Resources.Text3d;

namespace Realm.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureServer(this ServerBuilder builder, RealmConfigurationProvider realmConfigurationProvider)
    {
        var _serverConfiguration = realmConfigurationProvider.GetRequired<SlipeServer.Server.Configuration>("Server");
        builder.UseConfiguration(_serverConfiguration);
        var exceptBehaviours = ServerBuilderDefaultBehaviours.DefaultChatBehaviour;
#if DEBUG
        builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour | exceptBehaviours);
#else
        builder.AddDefaults(exceptBehaviours: exceptBehaviours);
#endif

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(realmConfigurationProvider);
            switch(realmConfigurationProvider.Get<string>("Database:Provider"))
            {
                case "MySql":
                    var connectionString = "server=localhost;port=3306;database=realm;uid=root;password=password";
                    services.AddPersistance<MySqlDb>(db => db.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
                    services.AddRealmIdentity<MySqlDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
                    break;
                case "SqlLite":
                    services.AddPersistance<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));
                    services.AddRealmIdentity<SQLiteDb>(realmConfigurationProvider.GetRequired<IdentityConfiguration>("Identity"));
                    break;
                default:
                    throw new NotImplementedException();
            }

            services.AddSingleton<HelpCommand>();
            services.AddSingleton<ICommand, TestCommand>();
            services.AddSingleton<ICommand, SaveCommand>();
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
        builder.AddClientInterfaceResource(commonOptions);
        builder.AddElementOutlineResource();
        builder.AddAdminToolsResource();
        builder.AddAFKResource();
        builder.AddStatisticsCounterResource();
        builder.AddOverlayResource();
        builder.AddAssets();
        builder.AddText3dResource();

        // Resources logics
        builder.AddLogic<ClientInterfaceLogic>();

        // Miscellaneous logic
        builder.AddLogic<CommandsLogic>();

        return builder;
    }
}
