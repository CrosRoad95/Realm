namespace RealmCore.Resources.ClientInterface;

public interface IClientInterfaceEventHub
{
    void AddFocusable();
    void RemoveFocusable();
    void AddFocusables(IEnumerable<Element> elements);
    void SetFocusableRenderingEnabled(bool enabled);
    void SetClipboard(string content);
    void SetDevelopmentModeEnabled(bool enabled);
}
