namespace RealmCore.Module.Grpc.Options;

public sealed class GrpcOptions
{
    public string Host { get; set; }
    public ushort Port { get; set; }
}

//public class GrpcServicesOptions
//{
//    public Dictionary<string, GrpcOptions> Services { get; set; }
//}
