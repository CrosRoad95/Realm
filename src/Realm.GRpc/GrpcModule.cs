using Realm.Module.Grpc.Services;
using Realm.Module.Grpc.Stubs;
using Realm.Module.Grpc.Stubs.Discord;

namespace Realm.Module.Grpc;

public class GrpcModule : IModule
{
    private Server? _grpcServer;

    public GrpcModule()
    {
    }

    public string Name => "Grpc";

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<GreeterServiceStub>();
        services.AddSingleton<DiscordHandshakeServiceStub>();
        services.AddSingleton<DiscordStatusChannelServiceStub>();
        services.AddSingleton<IGrpcDiscord, DiscordService>();
    }

    public void Init(IServiceProvider serviceProvider)
    {
        _grpcServer = new Server
        {
            Services =
            {
                Greet.Greeter.BindService(serviceProvider.GetRequiredService<GreeterServiceStub>()),
                Discord.Handshake.BindService(serviceProvider.GetRequiredService<DiscordHandshakeServiceStub>()),
                Discord.StatusChannel.BindService(serviceProvider.GetRequiredService<DiscordStatusChannelServiceStub>()),
            },
            Ports =
            {
                new ServerPort("localhost", 22010, ServerCredentials.Insecure)
            },
        };
    }

    public void PostInit(IServiceProvider serviceProvider)
    {
        Start();

        var logger = serviceProvider.GetRequiredService<ILogger>().ForContext<GrpcModule>();
    }

    public T GetInterface<T>() where T : class
    {

        throw new ArgumentException();
    }

    private void Start()
    {
        if (_grpcServer != null)
            _grpcServer.Start();
    }

    private void Shutdown()
    {
        if (_grpcServer != null)
            _grpcServer.ShutdownAsync().Wait();
    }

    public void Reload()
    {
        Shutdown();
        Start();
    }

    public int GetPriority() => -100;
}
