using SlipeServer.Server.Elements;
using System.Globalization;

namespace Realm.Resources.ClientInterface;

internal sealed class ClientInterfaceService : IClientInterfaceService
{
    public event Action<Player, string, int, string, int>? ClientErrorMessage;
    public event Action<Player, CultureInfo>? ClientCultureInfoChanged;
    public event Action<Player, int, int>? ClientScreenSizeChanged;
    public event Action<Player, Element?>? FocusedElementChanged;
    public event Action<Element>? FocusableAdded;
    public event Action<Element>? FocusableRemoved;
    public event Action<Player, bool>? FocusableRenderingChanged;

    public ClientInterfaceService()
    {

    }

    public void BroadcastClientErrorMessage(Player player, string message, int level, string file, int line)
    {
        ClientErrorMessage?.Invoke(player, message, level, file, line);
    }

    public void BroadcastPlayerLocalizationCode(Player player, string code)
    {
        ClientCultureInfoChanged?.Invoke(player, CultureInfo.GetCultureInfo(code));
    }

    public void BroadcastPlayerScreenSize(Player player, int x, int y)
    {
        ClientScreenSizeChanged?.Invoke(player, x, y);
    }

    public void BroadcastPlayerElementFocusChanged(Player player, Element? newFocusedElement, string? childElement)
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
