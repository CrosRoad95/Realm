using SlipeServer.Server.Elements;
using System.Globalization;

namespace Realm.Resources.LuaInterop;

public class LuaInteropService
{
    public event Action<Player, string, int, string, int>? ClientErrorMessage;
    public event Action<Player, CultureInfo>? ClientCultureInfoUpdate;
    public LuaInteropService()
    {

    }

    internal void BroadcastClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        ClientErrorMessage?.Invoke(player, message, level, file, line);
    }
    
    internal void BroadcastPlayerLocalizationCode(Player player, string code)
    {
        ClientCultureInfoUpdate?.Invoke(player, CultureInfo.GetCultureInfo(code));
    }

    public void SetWorldDebuggingEnabled(Player player, bool active)
    {
        player.TriggerLuaEvent("internalSetWorldDebuggingEnabled", player, active);
    }

    public void SetClipboard(Player player, string content)
    {
        player.TriggerLuaEvent("internalSetClipboard", player, content);
    }


}
