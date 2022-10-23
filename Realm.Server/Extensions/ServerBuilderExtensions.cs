namespace Realm.Server.Extensions;

public static class ServerBuilderExtensions
{
    public static ServerBuilder ConfigureServer(this ServerBuilder builder, IConfiguration configuration)
    {
        var _serverConfiguration = configuration.GetSection("server").Get<SlipeServerConfiguration>();
        var _scriptingConfiguration = configuration.GetSection("scripting").Get<ScriptingConfiguration>();
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
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            services.AddPersistance<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));
            services.AddRealmIdentity<SQLiteDb>(configuration.GetSection("Identity").Get<IdentityConfiguration>());

            services.AddSingleton<HelpCommand>();
            services.AddSingleton<ICommand, TestCommand>();
            services.AddSingleton<ICommand, ReloadCommand>();
        });

        builder.AddLogic<ClientInterfaceLogic>();
        builder.AddLogic<ClientUILogic>();
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
