namespace Realm.Server.Logic;

internal class StartupLogic
{
    public class ServerListConfiguration
    {
        public string GameType { get; set; } = "New-Realm";
        public string MapName { get; set; } = "N/A";
    }

    public StartupLogic(MtaServer mtaServer, IRealmConfigurationProvider realmConfigurationProvider)
    {
        var serverListConfiguration = realmConfigurationProvider.GetRequired<ServerListConfiguration>("ServerList");
        mtaServer.GameType = serverListConfiguration.GameType;
        mtaServer.MapName = serverListConfiguration.MapName;
    }
}
