using System.Globalization;

namespace RealmCore.Resources.ClientInterface;

public interface IClientInterfaceService
{
    event Action<Player, string, int, string, int>? ClientErrorMessage;
    event Action<Player, CultureInfo>? ClientCultureInfoChanged;
    event Action<Player, int, int>? ClientScreenSizeChanged;
    event Action<Player, Element?, string?>? FocusedElementChanged;
    event Action<Player, Element?>? ClickedElementChanged;

    internal event Action<Element>? FocusableAdded;
    internal event Action<Element>? FocusableRemoved;
    internal event Action<Player, bool>? FocusableRenderingChanged;
    internal event Action<Element, Player>? FocusableForAdded;
    internal event Action<Element, Player>? FocusableForRemoved;

    void AddFocusable(Element element);
    void RemoveFocusable(Element element);
    void SetClipboard(Player player, string content);
    void SetDevelopmentModeEnabled(Player player, bool enabled);
    void SetFocusableRenderingEnabled(Player player, bool enabled);
    void SetWorldDebuggingEnabled(Player player, bool active);

    internal void BroadcastClientErrorMessage(Player player, string message, int level, string file, int line);
    internal void BroadcastPlayerLocalizationCode(Player player, string code);
    internal void BroadcastPlayerScreenSize(Player player, int x, int y);
    internal void BroadcastPlayerElementFocusChanged(Player player, Element? newFocusedElement, string? childElement);
    void BroadcastClickedElementChanged(Player player, Element? clickedElement);
    void AddFocusableFor(Element element, Player player);
    void RemoveFocusableFor(Element element, Player player);
}

internal sealed class ClientInterfaceService : IClientInterfaceService
{
    public event Action<Player, string, int, string, int>? ClientErrorMessage;
    public event Action<Player, CultureInfo>? ClientCultureInfoChanged;
    public event Action<Player, int, int>? ClientScreenSizeChanged;
    public event Action<Player, Element?, string?>? FocusedElementChanged;
    public event Action<Player, Element?>? ClickedElementChanged;
    public event Action<Element>? FocusableAdded;
    public event Action<Element>? FocusableRemoved;
    public event Action<Element, Player>? FocusableForAdded;
    public event Action<Element, Player>? FocusableForRemoved;
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
        FocusedElementChanged?.Invoke(player, newFocusedElement, childElement);
    }
    
    public void BroadcastClickedElementChanged(Player player, Element? clickedElement)
    {
        ClickedElementChanged?.Invoke(player, clickedElement);
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

    public void AddFocusableFor(Element element, Player player)
    {
        FocusableForAdded?.Invoke(element, player);
    }

    public void RemoveFocusableFor(Element element, Player player)
    {
        FocusableForRemoved?.Invoke(element, player);
    }

    public void SetFocusableRenderingEnabled(Player player, bool enabled)
    {
        FocusableRenderingChanged?.Invoke(player, enabled);
    }
}
