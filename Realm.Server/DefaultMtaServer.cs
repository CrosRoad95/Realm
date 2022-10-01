namespace Realm.Server;

public partial class DefaultMtaServer
{
    private readonly EventWaitHandle _waitHandle = new(false, EventResetMode.AutoReset);
    private readonly MtaServer<DefaultPlayer> _server;
    private readonly Configuration _serverConfiguration;
    private readonly ScriptingConfiguration _scriptingConfiguration;

    public ILogger Logger { get; }

    public DefaultMtaServer(IConfiguration configuration)
    {
        _serverConfiguration = configuration.GetSection("server").Get<Configuration>();
        _scriptingConfiguration = configuration.GetSection("scripting").Get<ScriptingConfiguration>();

        _server = MtaServer.CreateWithDiSupport<DefaultPlayer>(
            builder =>
            {
                builder.UseConfiguration(_serverConfiguration);

#if DEBUG
                builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
                builder.AddNetWrapper(dllPath: "net_d", port: (ushort)(_serverConfiguration.Port + 1));
#else
                builder.AddDefaults();
#endif
                builder.AddResourceWithAutostart<ClientInterfaceResource, IClientInterface, ClientInterfaceLogic>();

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<Startup>();
                    if(_scriptingConfiguration.Enabled)
                        services.AddScripting();
                    services.AddPersistance<SQLiteDb>(db => db.UseSqlite("Filename=./server.db"));
                });
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
            _waitHandle.Set();
        };
    }

    public void InitializeScripting(string fileName)
    {
        var code = File.ReadAllText(fileName);
        var scripting = _server.GetRequiredService<IScripting>();
        scripting.Execute(code);
    }

    public void Start()
    {
        if(_scriptingConfiguration.Enabled)
            InitializeScripting("Resources/startup.js");

        Logger.LogInformation("Server started.");
        _server.PlayerJoined += OnPlayerJoin;
        _server.Start();
        _waitHandle.WaitOne();
    }

    private void OnPlayerJoin(DefaultPlayer player)
    {
        player.Camera.Target = player;
        player.Camera.Fade(CameraFade.In);
        player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
    }
}