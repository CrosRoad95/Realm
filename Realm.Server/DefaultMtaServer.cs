using Realm.Server.ResourcesLogic;

namespace Realm.Server;

public partial class DefaultMtaServer
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
    private readonly MtaServer<RPGPlayer> _server;
    private readonly Configuration _serverConfiguration;
    private readonly ScriptingConfiguration _scriptingConfiguration;

    public ILogger Logger { get; }

    public DefaultMtaServer(IConfiguration configuration, Action<ServerBuilder>? configureServerBuilder = null)
    {
        _serverConfiguration = configuration.GetSection("server").Get<Configuration>();
        _scriptingConfiguration = configuration.GetSection("scripting").Get<ScriptingConfiguration>();

        _server = MtaServer.CreateWithDiSupport<RPGPlayer>(
            builder =>
            {
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
                    if(_scriptingConfiguration.Enabled)
                        services.AddScripting();
                    services.AddDiscord();
                    services.AddPersistance<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));

                    services.AddSingleton<HelpCommand>();
                    services.AddSingleton<ICommand, TestCommand>();
                });

                builder.AddLogic<CommandsLogic>();

                if (configureServerBuilder != null)
                    configureServerBuilder(builder);
            }
        );

        var serverListConfiguration = configuration.GetSection("serverList").Get<ServerListConfiguration>();
        _server.GameType = serverListConfiguration.GameType;
        _server.MapName = serverListConfiguration.MapName;

        Logger = _server.GetRequiredService<ILogger>();

        var startup = _server.GetRequiredService<Startup>();
        var _ = Task.Run(startup.StartAsync);

        Console.CancelKeyPress += (sender, args) =>
        {
            _server.Stop();
            _semaphore.Release();
        };
    }

    public void InitializeScripting(string fileName)
    {
        var code = File.ReadAllText(fileName);
        var scripting = _server.GetRequiredService<IScripting>();
        scripting.Execute(code);
    }

    public async Task Start()
    {
        if(_scriptingConfiguration.Enabled)
            InitializeScripting("Resources/startup.js");

        Logger.LogInformation("Server started.");
        _server.PlayerJoined += OnPlayerJoin;
        _server.Start();
        await _semaphore.WaitAsync();
    }

    private void OnPlayerJoin(RPGPlayer player)
    {
        player.Camera.Target = player;
        player.Camera.Fade(CameraFade.In);
        player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
    }
}