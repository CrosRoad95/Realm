namespace Realm.Server.Logic;

internal class StartupLogic
{
    public StartupLogic(MtaServer mtaServer, RealmConfigurationProvider realmConfigurationProvider)
    {
        var serverListConfiguration = realmConfigurationProvider.GetRequired<ServerListConfiguration>("ServerList");
        mtaServer.GameType = serverListConfiguration.GameType;
        mtaServer.MapName = serverListConfiguration.MapName;
    }
}
