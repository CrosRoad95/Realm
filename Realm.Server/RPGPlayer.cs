using SlipeServer.Packets.Definitions.Lua;
using SlipeServer.Server.Mappers;
using System.Linq;

namespace Realm.Server;

public class RPGPlayer : Player, IRPGPlayer
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly LuaValueMapper _luaValueMapper;

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

    public void TriggerClientEvent(string name, object value)
    {
        LuaValue luaValue = _luaValueMapper.Map(value);
        TriggerLuaEvent(name, this, luaValue);
    }
    public override string ToString() => "Player";
}
