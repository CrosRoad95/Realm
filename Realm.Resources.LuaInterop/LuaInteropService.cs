using SlipeServer.Server.Elements;

namespace Realm.Resources.LuaInterop;

public class LuaInteropService
{
    public event Action<Player, string, int, string, int>? ClientErrorMessage;
    public LuaInteropService()
    {

    }

    internal void BroadcastClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        ClientErrorMessage?.Invoke(player, message, level, file, line);
    }

    public void SetWorldDebuggingEnabled(Player player, bool active)
    {
        player.TriggerLuaEvent("internalSetWorldDebuggingEnabled", player, active);
    }
}
