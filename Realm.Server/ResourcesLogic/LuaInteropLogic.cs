namespace Realm.Server.ResourcesLogic;

internal class LuaInteropLogic
{
    private readonly Resource _resource;
    private readonly ILogger _logger;
    private readonly FromLuaValueMapper _fromLuaValueMapper;
    private readonly List<RPGPlayer> _startedForPlayers = new();

    public LuaInteropLogic(IResourceProvider resourceProvider, IRPGServer rpgServer, ILogger logger, FromLuaValueMapper fromLuaValueMapper)
    {
        _logger = logger.ForContext<LuaInteropLogic>();
        _resource = resourceProvider.GetResource("LuaInterop");

        rpgServer.PlayerJoined += Start;
        rpgServer.ServerReloaded += RPGServer_ServerReloaded;

        //rpgServer.AddEventHandler("internalDebugMessage", InternalDebugMessage);
        _fromLuaValueMapper = fromLuaValueMapper;
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
        _resource.StartFor(player);
        _startedForPlayers.Add(rpgPlayer);
        rpgPlayer.Disconnected += RPGPlayerDisconnected;
        rpgPlayer.DebugWorldChanged += RPGPlayerDebugWorldActiveChanged;
    }

    private void RPGPlayerDisconnected(Player player, PlayerQuitEventArgs e)
    {
        _startedForPlayers.Remove((RPGPlayer)player);
    }

    private void RPGPlayerDebugWorldActiveChanged(RPGPlayer player, bool active)
    {
        player.TriggerClientEvent("internalSetWorldDebuggingEnabled", active);
    }

    private Task<object?> InternalDebugMessage(LuaEvent luaEvent)
    {
        var message = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[1]) as string;
        var level = _fromLuaValueMapper.Map(typeof(int), luaEvent.Parameters[2]);
        var file = _fromLuaValueMapper.Map(typeof(string), luaEvent.Parameters[3]) as string;
        var line = _fromLuaValueMapper.Map(typeof(int), luaEvent.Parameters[4]);
        _logger.Warning("Got client side script error on player {player} ({level}): {message} in {file}:{line}", (RPGPlayer)luaEvent.Player, level, message, file, line);
        return Task.FromResult<object?>(null);
    }
}
