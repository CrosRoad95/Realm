namespace RealmCore.Resources.CEFBlazorGui;

public interface ICEFBlazorGuiEventHub
{
    void Load(string mode, int x, int y);
    void SetDevelopmentMode(bool enabled);
    void ToggleDevTools(bool enabled);
    void SetVisible(bool enabled);
    void SetPath(string path, bool force);
    void InvokeAsyncSuccess(string promiseId, string data);
    void InvokeAsyncError(string promiseId);
}
