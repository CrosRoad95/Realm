using SlipeServer.Server.Elements;

namespace RealmCore.Resources.ClientInterface;

public interface IClientInterfaceEventHub
{
    void AddFocusable();
    void RemoveFocusable();
    void AddFocusables(IEnumerable<Element> elements);
    void SetFocusableRenderingEnabled(bool enabled);
}
