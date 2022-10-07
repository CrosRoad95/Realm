using Realm.Server.Data.Configurations;
using Realm.Server.Managers;
using Realm.Server.ResourcesLogic;

namespace Realm.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureServer(this ServerBuilder builder, IConfiguration configuration)
    {
        var _serverConfiguration = configuration.GetSection("server").Get<Configuration>();
        var _scriptingConfiguration = configuration.GetSection("scripting").Get<ScriptingConfiguration>();
        builder.UseConfiguration(_serverConfiguration);
#if DEBUG
        builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
        builder.AddNetWrapper(dllPath: "net_d", port: (ushort)(_serverConfiguration.Port + 1));
#else
                builder.AddDefaults();
#endif

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(configuration);
            services.AddSingleton<Startup>();
            services.AddSingleton<IAutoStartResource, ClientInterfaceLogic>();
            services.AddSingleton<IAutoStartResource, ClientUILogic>();

            services.AddSingleton<ISpawnManager, SpawnManager>();
            if (_scriptingConfiguration.Enabled)
                services.AddScripting();
            services.AddDiscord();
            services.AddPersistance<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));

            services.AddSingleton<HelpCommand>();
            services.AddSingleton<ICommand, TestCommand>();
        });

        builder.AddLogic<CommandsLogic>();

        return builder;
    }

    public static ServerBuilder AddGuiFilesLocation(this ServerBuilder serverBuilder, string path = "Gui")
    {
        serverBuilder.ConfigureServices(services =>
        {
            services.AddSingleton<IGuiFilesProvider>(new GuiFilesProvider(path));
        });
        return serverBuilder;
    }
}
