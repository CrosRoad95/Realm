namespace RealmCore.Resources.CEFBlazorGui;

public interface ICEFBlazorGuiEventHub
{
    void Load(string mode, int x, int y);
    void SetDevelopmentMode(bool enabled);
    void ToggleDevTools(bool enabled);
    void SetVisible(bool enabled);
    void SetPath(string path);
}
