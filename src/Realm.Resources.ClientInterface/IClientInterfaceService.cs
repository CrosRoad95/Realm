using SlipeServer.Server.Elements;
using System.Globalization;

namespace Realm.Resources.ClientInterface;

public interface IClientInterfaceService
{
    event Action<Player, string, int, string, int>? ClientErrorMessage;
    event Action<Player, CultureInfo>? ClientCultureInfoChanged;
    event Action<Player, int, int>? ClientScreenSizeChanged;
    event Action<Player, Element?>? FocusedElementChanged;

    internal event Action<Element>? FocusableAdded;
    internal event Action<Element>? FocusableRemoved;
    internal event Action<Player, bool>? FocusableRenderingChanged;

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
}
