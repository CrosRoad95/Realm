namespace RealmCore.Server.Logic;

internal class StartupLogic
{
    public StartupLogic(MtaServer mtaServer, IOptions<ServerListOptions> serverListOptions)
    {
        mtaServer.GameType = serverListOptions.Value.GameType;
        mtaServer.MapName = serverListOptions.Value.MapName;
    }
}
