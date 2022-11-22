using Realm.Resources.Addons.AgnosticGuiSystem.DGSProvider;
using Realm.Resources.AdminTools;
using RealmResources.ElementOutline;
using SlipeServer.Resources.DGS;

namespace Realm.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureServer(this ServerBuilder builder, IConfiguration configuration, string? basePath = null)
    {
        var _serverConfiguration = configuration.GetSection("server").Get<SlipeServerConfiguration>();
        var _scriptingConfiguration = configuration.GetSection("scripting").Get<ScriptingConfiguration>();
        if (basePath != null)
            _serverConfiguration.ResourceDirectory = Path.Join(basePath, _serverConfiguration.ResourceDirectory);
        builder.UseConfiguration(_serverConfiguration);
#if DEBUG
        builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
#else
                builder.AddDefaults();
#endif

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(configuration);
            services.AddSingleton<Startup>();

            services.AddPersistance<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));
            services.AddRealmIdentity<SQLiteDb>(configuration.GetSection("Identity").Get<IdentityConfiguration>());

            services.AddSingleton<HelpCommand>();
            services.AddSingleton<ICommand, TestCommand>();
            services.AddSingleton<ICommand, ReloadCommand>();
        });

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

        builder.AddLogic<LuaInteropLogic>();
        builder.AddLogic<ClientUILogic>();
        builder.AddLogic<AdminToolsLogic>();

        builder.AddLogic<CommandsLogic>();

        return builder;
    }
}
