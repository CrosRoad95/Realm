using Grpc.Core.Logging;
using Microsoft.Extensions.Logging;
using Realm.Module.Grpc.Services;
using Realm.Module.Grpc.Stubs;
using Realm.Module.Grpc.Stubs.Discord;

namespace Realm.Module.Grpc;

public class GrpcModule : IModule
{
    private Server? _grpcServer;
    private ILogger<GrpcModule> _logger;
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
        _logger = serviceProvider.GetRequiredService<ILogger<GrpcModule>>();
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
        Start();
    }

    public T GetInterface<T>() where T : class
    {

        throw new ArgumentException();
    }

    private void Start()
    {
        if (_grpcServer != null)
        {
            _grpcServer.Start();
            _logger.LogInformation("Started grpc server.");
        }
    }

    private void Shutdown()
    {
        if (_grpcServer != null)
            _grpcServer.ShutdownAsync().Wait();
    }
}
