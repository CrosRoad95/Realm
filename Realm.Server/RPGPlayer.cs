namespace Realm.Server;

public class RPGPlayer : Player, IRPGPlayer
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly LuaValueMapper _luaValueMapper;

    public bool IsPersistant => true;
    public new string Id => Name;
    public CancellationToken CancellationToken { get; private set; }
    public event Action<IRPGPlayer, int>? ResourceReady;
    public RPGPlayer(LuaValueMapper luaValueMapper)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken = _cancellationTokenSource.Token;
        ResourceStarted += RPGPlayer_ResourceStarted;
        Disconnected += RPGPlayer_Disconnected;
        _luaValueMapper = luaValueMapper;
    }

    private void RPGPlayer_Disconnected(Player sender, PlayerQuitEventArgs e)
    {
        _cancellationTokenSource.Cancel();
    }

    private void RPGPlayer_ResourceStarted(Player sender, PlayerResourceStartedEventArgs e)
    {
        ResourceReady?.Invoke(sender as RPGPlayer, e.NetId);
    }

    public void Spawn(ISpawn spawn)
    {
        Camera.Target = this;
        Camera.Fade(CameraFade.In);
        Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
    }

    public void TriggerClientEvent(string name, params object[] values)
    {
        LuaValue[] luaValue = values.Select(_luaValueMapper.Map).ToArray();
        TriggerLuaEvent(name, this, luaValue);
    }
    public override string ToString() => "Player";
}
