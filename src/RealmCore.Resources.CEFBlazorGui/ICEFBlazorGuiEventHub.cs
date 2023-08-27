namespace RealmCore.Resources.CEFBlazorGui;

internal interface ICEFBlazorGuiEventHub
{
    void Load(string mode, int x, int y, string? remoteUrl, string? requestWhitelistUrl);
    void ToggleDevTools(bool enabled);
    void SetVisible(bool enabled);
    void SetPath(string path, bool force, bool isAsync);
    void SetRemotePath(string path);
    void InvokeAsyncSuccess(string promiseId, string data);
    void InvokeAsyncError(string promiseId, string reason);
}
