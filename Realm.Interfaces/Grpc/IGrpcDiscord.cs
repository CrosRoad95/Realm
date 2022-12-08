namespace Realm.Interfaces.Grpc;

public delegate Task<string> UpdateStatusChannel();

public interface IGrpcDiscord
{
    UpdateStatusChannel? UpdateStatusChannel { get; set; }
}
