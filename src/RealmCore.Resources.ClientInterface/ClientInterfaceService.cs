using System.Globalization;

namespace RealmCore.Resources.ClientInterface;

public record struct ClientDebugMessage(string Message, int Level, string File, int Line);

public interface IClientInterfaceService
{
    event Action<Player, ClientDebugMessage[]>? OnClientDebugMessages;
    event Action<Player, CultureInfo>? ClientCultureInfoChanged;
    event Action<Player, int, int>? ClientScreenSizeChanged;
    event Action<Player, Element?, string?>? FocusedElementChanged;
    event Action<Player, Element?>? ClickedElementChanged;

    internal event Action<Element>? FocusableAdded;
    internal event Action<Element>? FocusableRemoved;
    internal event Action<Player, bool>? FocusableRenderingChanged;
    internal event Action<Element, Player>? FocusableForAdded;
    internal event Action<Element, Player>? FocusableForRemoved;
    internal event Action<Player, string>? ClipboardChanged;
    internal event Action<Player, bool>? DevelopmentModeChanged;

    void AddFocusable(Element element);
    void RemoveFocusable(Element element);
    void SetClipboard(Player player, string content);
    void SetDevelopmentModeEnabled(Player player, bool enabled);
    void SetFocusableRenderingEnabled(Player player, bool enabled);

    internal void RelayClienDebugMessages(Player player, ClientDebugMessage[] debugMessages);
    internal void RelayPlayerLocalizationCode(Player player, string code);
    internal void RelayPlayerScreenSize(Player player, int x, int y);
    internal void RelayPlayerElementFocusChanged(Player player, Element? newFocusedElement, string? childElement);
    void RelayClickedElementChanged(Player player, Element? clickedElement);
    void AddFocusableFor(Element element, Player player);
    void RemoveFocusableFor(Element element, Player player);
}

internal sealed class ClientInterfaceService : IClientInterfaceService
{
    public event Action<Player, ClientDebugMessage[]>? OnClientDebugMessages;
    public event Action<Player, CultureInfo>? ClientCultureInfoChanged;
    public event Action<Player, int, int>? ClientScreenSizeChanged;
    public event Action<Player, Element?, string?>? FocusedElementChanged;
    public event Action<Player, Element?>? ClickedElementChanged;
    public event Action<Element>? FocusableAdded;
    public event Action<Element>? FocusableRemoved;
    public event Action<Element, Player>? FocusableForAdded;
    public event Action<Element, Player>? FocusableForRemoved;
    public event Action<Player, bool>? FocusableRenderingChanged;
    public event Action<Player, string>? ClipboardChanged;
    public event Action<Player, bool>? DevelopmentModeChanged;

    public void RelayClienDebugMessages(Player player, ClientDebugMessage[] debugMessages)
    {
        OnClientDebugMessages?.Invoke(player, debugMessages);
    }

    public void RelayPlayerLocalizationCode(Player player, string code)
    {
        ClientCultureInfoChanged?.Invoke(player, CultureInfo.GetCultureInfo(code));
    }

    public void RelayPlayerScreenSize(Player player, int x, int y)
    {
        ClientScreenSizeChanged?.Invoke(player, x, y);
    }

    public void RelayPlayerElementFocusChanged(Player player, Element? newFocusedElement, string? childElement)
    {
        FocusedElementChanged?.Invoke(player, newFocusedElement, childElement);
    }
    
    public void RelayClickedElementChanged(Player player, Element? clickedElement)
    {
        ClickedElementChanged?.Invoke(player, clickedElement);
    }

    public void SetClipboard(Player player, string content)
    {
        ClipboardChanged?.Invoke(player, content);
    }

    public void SetDevelopmentModeEnabled(Player player, bool enabled)
    {
        DevelopmentModeChanged?.Invoke(player, enabled);
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
