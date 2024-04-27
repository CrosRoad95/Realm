namespace RealmCore.Module.Grpc.Options;

public sealed class GrpcOptions
{
    public required string Host { get; set; }
    public required ushort Port { get; set; }
    public required string RemoteHost { get; init; }
    public required ushort RemotePort { get; set; }
}
