namespace RealmCore.Resources.Browser;

internal interface IBrowserEventHub
{
    void Load(int x, int y, string remoteUrl, string requestWhitelistUrl);
    void ToggleDevTools(bool enabled);
    void SetVisible(bool enabled);
    void SetPath(string path);
}
