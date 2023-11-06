namespace RealmCore.Server.Interfaces.Players;

public interface IPlayerBrowserService
{
    bool Visible { get; set; }
    bool DevTools { get; set; }

    event Action<string, bool>? PathChanged;
    event Action<bool>? DevToolsStateChanged;
    event Action<bool>? VisibleChanged;

    void Close();
    void Open(string path);
    void SetPath(string path, bool clientSide = false);
}
