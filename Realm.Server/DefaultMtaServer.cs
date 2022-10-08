using Realm.Interfaces.Scripting;
using Realm.Server.Extensions;

namespace Realm.Server;

public partial class DefaultMtaServer : IReloadable
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);
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
                builder.ConfigureServer(configuration);
                if (configureServerBuilder != null)
                    configureServerBuilder(builder);

                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IReloadable>(this);
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
            _semaphore.Release();
        };
    }

    public void InitializeScripting(string fileName)
    {
        var code = File.ReadAllText(fileName);
        var scripting = _server.GetRequiredService<IScripting>();
        scripting.Execute(code, fileName);

        var typescriptDefinitions = scripting.GetTypescriptDefinition();
        var directory = Path.GetDirectoryName(fileName);
        File.WriteAllText(Path.Join(directory,"types.ts"), typescriptDefinitions);
    }

    private void StartScripting()
    {
        InitializeScripting("Server/startup.js");
    }

    public async Task Start()
    {
        if (_scriptingConfiguration.Enabled)
            StartScripting();

        _server.PlayerJoined += OnPlayerJoin;
        Console.WriteLine("Server started at port: {0}", _serverConfiguration.Port);
        _server.Start();
        await _semaphore.WaitAsync();
    }

    private void OnPlayerJoin(RPGPlayer player)
    {
        player.Camera.Target = player;
        player.Camera.Fade(CameraFade.In);
        player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
    }

    public void Reload()
    {
        StartScripting();
    }

    public int GetPriority() => int.MaxValue;
}