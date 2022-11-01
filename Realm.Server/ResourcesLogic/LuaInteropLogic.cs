namespace Realm.Server.ResourcesLogic;

internal class LuaInteropLogic
{
    private readonly Resource _resource;
    private readonly List<RPGPlayer> _startedForPlayers = new();

    public LuaInteropLogic(IResourceProvider resourceProvider, IRPGServer rpgServer)
    {
        _resource = resourceProvider.GetResource("LuaInterop");
        _resource.AddGlobals();

        rpgServer.PlayerJoined += Start;
        rpgServer.ServerReloaded += RPGServer_ServerReloaded;
    }

    private void RPGServer_ServerReloaded()
    {
        foreach (var player in _startedForPlayers)
        {
            _resource.StopFor(player);
        }
        foreach (var player in _startedForPlayers)
        {
            _resource.StartFor(player);
        }
    }

    public void Start(Player player)
    {
        var rpgPlayer = (RPGPlayer)player;
        rpgPlayer.ResourceStartingLatch.Increment();
        _resource.StartFor(player);
        _startedForPlayers.Add(rpgPlayer);
        player.Disconnected += Player_Disconnected;
    }

    private void Player_Disconnected(Player player, PlayerQuitEventArgs e)
    {
        _startedForPlayers.Remove((RPGPlayer)player);
    }
}
