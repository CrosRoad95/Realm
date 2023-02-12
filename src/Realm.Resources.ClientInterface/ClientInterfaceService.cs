using SlipeServer.Server.Elements;
using System.Globalization;

namespace Realm.Resources.ClientInterface;

public class ClientInterfaceService
{
    public event Action<Player, string, int, string, int>? ClientErrorMessage;
    public event Action<Player, CultureInfo>? ClientCultureInfoChanged;
    public event Action<Player, int, int>? ClientScreenSizeChanged;
    public event Action<Player, Element?>? FocusedElementChanged;
    internal event Action<Element>? FocusableAdded;
    internal event Action<Element>? FocusableRemoved;
    internal event Action<Player, bool>? FocusableRenderingChanged;

    public ClientInterfaceService()
    {

    }

    internal void BroadcastClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        ClientErrorMessage?.Invoke(player, message, level, file, line);
    }
    
    internal void BroadcastPlayerLocalizationCode(Player player, string code)
    {
        ClientCultureInfoChanged?.Invoke(player, CultureInfo.GetCultureInfo(code));
    }
    
    internal void BroadcastPlayerScreenSize(Player player, int x, int y)
    {
        ClientScreenSizeChanged?.Invoke(player, x, y);
    }
    
    internal void BroadcastPlayerElementFocusChanged(Player player, Element? newFocusedElement)
    {
        FocusedElementChanged?.Invoke(player, newFocusedElement);
    }

    public void SetWorldDebuggingEnabled(Player player, bool active)
    {
        player.TriggerLuaEvent("internalSetWorldDebuggingEnabled", player, active);
    }

    public void SetClipboard(Player player, string content)
    {
        player.TriggerLuaEvent("internalSetClipboard", player, content);
    }
    
    public void SetDevelopmentModeEnabled(Player player, bool enabled)
    {
        player.TriggerLuaEvent("internalSetDevelopmentModeEnabled", player, enabled);
    }

    public void AddFocusable(Element element)
    {
        FocusableAdded?.Invoke(element);
    }

    public void RemoveFocusable(Element element)
    {
        FocusableRemoved?.Invoke(element);
    }

    public void SetFocusableRenderingEnabled(Player player, bool enabled)
    {
        FocusableRenderingChanged?.Invoke(player, enabled);
    }
}
