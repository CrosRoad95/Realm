using Grpc.Net.Client;

namespace Realm.WebApp.Services;

public class SettingsService
{
    public string ServerName { get; set; }
    public int FpsLimit { get; set; }
    public int Weather { get; set; }
    public string GameType { get; set; }
    public string MapName { get; set; }

    public SettingsService(GrpcChannel grpcChannel)
    {

    }
}
