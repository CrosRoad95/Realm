using Realm.GRpc.Stubs.Discord;

namespace Realm.GRpc;

public class GrpcModule : IModule
{
    private Server? _grpcServer;

    public GrpcModule()
    {
    }

    public string Name => "Grpc";
    private IGrpcModuleInterface? _interface;

    public void Configure(IServiceCollection services)
    {
        services.AddSingleton<GreeterServiceStub>();
        services.AddSingleton<DiscordHandshakeServiceStub>();
        services.AddSingleton<DiscordStatusChannelServiceStub>();
    }

    public void Init(IServiceProvider serviceProvider)
    {
        Start();
        _interface = serviceProvider.GetRequiredService<IGrpcModuleInterface>();

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
                new ServerPort("0.0.0.0", 22010, ServerCredentials.Insecure)
            },
        };
    }

    public void PostInit(IServiceProvider serviceProvider)
    {
        if (_interface == null)
            throw new InvalidOperationException();
        Start();

        var logger = serviceProvider.GetRequiredService<ILogger>().ForContext<GrpcModule>();
    }

    public T GetInterface<T>() where T : class
    {
        if (_interface == null)
            throw new InvalidOperationException();

        if (typeof(T) == typeof(IGrpcModuleInterface))
        {
            var @interface = _interface as T;
            if (@interface == null)
                throw new InvalidOperationException();
            return @interface;
        }
        throw new ArgumentException();
    }

    private void Start()
    {
        if(_grpcServer != null)
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
