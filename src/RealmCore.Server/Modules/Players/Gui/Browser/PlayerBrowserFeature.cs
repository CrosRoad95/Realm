namespace RealmCore.Server.Modules.Players.Gui.Browser;

public interface IPlayerBrowserFeature : IPlayerFeature
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

internal sealed class PlayerBrowserFeature : IPlayerBrowserFeature, IDisposable
{
    public event Action<string, bool>? PathChanged;
    public event Action<bool>? DevToolsStateChanged;
    public event Action<bool>? VisibleChanged;

    private readonly IBrowserGuiService _browserGuiService;
    private readonly IBrowserService _browserService;

    private string _path = "/";
    public string Path
    {
        get => _path; set
        {
            PathChanged?.Invoke(value, false);
            _path = value;
        }
    }

    private bool _devTools;
    public bool DevTools
    {
        get => _devTools; set
        {
            if (_devTools != value)
            {
                _devTools = value;
                DevToolsStateChanged?.Invoke(value);
                _browserService.ToggleDevTools(Player, value);
            }
        }
    }

    private bool _visible;

    public bool Visible
    {
        get => _visible;
        set
        {
            if (_visible != value)
            {
                _visible = value;
                VisibleChanged?.Invoke(value);
                _browserService.SetVisible(Player, value);
            }
        }
    }

    public RealmPlayer Player { get; init; }

    public PlayerBrowserFeature(IBrowserGuiService browserGuiService, IBrowserService browserService, PlayerContext playerContext)
    {
        var key = browserGuiService.GenerateKey();
        browserGuiService.AuthorizePlayer(key, playerContext.Player);
        _browserGuiService = browserGuiService;
        _browserService = browserService;
        Player = playerContext.Player;
        browserService.BrowserStarted += HandleBrowserStarted;
    }

    private void HandleBrowserStarted(Player player)
    {
        if (player != Player)
            return;

        var key = _browserGuiService.GenerateKey();
        if (_browserGuiService.AuthorizePlayer(key, Player))
        {
            var url = $"/realmGuiInitialize?{_browserGuiService.KeyName}={key}";
            SetPath(url, true);
        }
    }

    public void SetPath(string path, bool clientSide = false)
    {
        PathChanged?.Invoke(path, clientSide);
        _path = path;
        if (clientSide)
            _browserService.SetPath(Player, _path);
    }

    /// <summary>
    /// Server side only
    /// </summary>
    /// <param name="path"></param>
    public void Open(string path)
    {
        Path = path;
        Visible = true;
    }

    public void Close()
    {
        Visible = false;
    }

    public void Dispose()
    {
        _browserGuiService.UnauthorizePlayer(Player);
    }
}
