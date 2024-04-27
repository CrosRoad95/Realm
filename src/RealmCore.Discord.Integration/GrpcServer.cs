using RealmCore.Module.Grpc;

namespace RealmCore.Discord.Integration;

internal sealed class GrpcServer : IGrpcServer
{
    private readonly Server _server;

    public GrpcServer(IOptions<GrpcOptions> grpcOptions, IEnumerable<ServerServiceDefinition> serverServiceDefinitions, ILogger<GrpcServer> logger)
    {
        _server = new Server
        {
            Ports = {
                new ServerPort(grpcOptions.Value.Host, grpcOptions.Value.Port, ServerCredentials.Insecure)
            },
        };
        foreach (var item in serverServiceDefinitions)
            _server.Services.Add(item);

        logger.LogInformation("Created Grpc server at address: {ip}:{port}", grpcOptions.Value.Host, grpcOptions.Value.Port);
    }

    public void Start() => _server.Start();
}
