namespace Realm.Server.Elements;

public class RPGPlayer : Player
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly LuaValueMapper _luaValueMapper;

    public CancellationToken CancellationToken { get; private set; }
    public event Action<RPGPlayer, int>? ResourceReady;
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

    public void Spawn(Spawn spawn)
    {
        Camera.Target = this;
        Camera.Fade(CameraFade.In);
        Spawn(spawn.Position, spawn.Rotation.Z, 0, 0, 0);
    }

    public bool IsPersistant() => true;

    // TODO: improve
    public void TriggerClientEvent(string name, params object[] values)
    {
        LuaValue[] luaValue;
        if (values.Length == 1 && values[0].GetType() == typeof(object[]))
        {
            luaValue = (values[0] as object[]).Select(_luaValueMapper.Map).ToArray();
        }
        else
        {
            luaValue = values.Select(_luaValueMapper.Map).ToArray();
        }
        TriggerLuaEvent(name, this, luaValue);
    }

    public override string ToString() => "Player";
}
