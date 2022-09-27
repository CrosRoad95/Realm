using SlipeServer.Packets.Lua.Camera;
using System.Numerics;

namespace Realm.Server;

public partial class DefaultMtaServer
{
    private readonly EventWaitHandle waitHandle = new(false, EventResetMode.AutoReset);
    private readonly MtaServer<DefaultPlayer> server;
    private readonly Configuration configuration;

    public ILogger Logger { get; }

    public DefaultMtaServer(string[] args)
    {
        configuration = new Configuration()
        {
            IsVoiceEnabled = true,
            ServerName = "Realm by CrosRoad95",
            Port = 22003,
            HttpPort = 22005,
            MaxPlayerCount = 4097,
        };

        server = MtaServer.CreateWithDiSupport<DefaultPlayer>(
            (builder) =>
            {
                builder.UseConfiguration(configuration);

#if DEBUG
                builder.AddDefaults(exceptBehaviours: ServerBuilderDefaultBehaviours.MasterServerAnnouncementBehaviour);
                builder.AddNetWrapper(dllPath: "net_d", port: (ushort)(configuration.Port + 1));
#else
                builder.AddDefaults();
#endif

                builder.ConfigureServices(services =>
                {
                });
            }
        );

        server.GameType = "Realm";
        server.MapName = "N/A";

        Logger = server.GetRequiredService<ILogger>();

        Console.CancelKeyPress += (sender, args) =>
        {
            server.Stop();
            waitHandle.Set();
        };
    }

    public void Start()
    {
        Logger.LogInformation("Server started.");
        server.PlayerJoined += OnPlayerJoin;
        server.Start();
        waitHandle.WaitOne();
    }

    private void OnPlayerJoin(DefaultPlayer player)
    {
        player.Camera.Target = player;
        player.Camera.Fade(CameraFade.In);
        player.Spawn(new Vector3(0, 0, 3), 0, 7, 0, 0);
    }
}