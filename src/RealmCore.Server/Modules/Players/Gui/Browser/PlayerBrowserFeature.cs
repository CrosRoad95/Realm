namespace RealmCore.Server.Modules.Players.Gui.Browser;

public interface IPlayerBrowserFeature : IPlayerFeature
{
    bool Visible { get; set; }
    bool DevTools { get; set; }
    string Key { get; init; }
    bool IsReady { get; }

    event Action<string, bool>? PathChanged;
    event Action<bool>? DevToolsStateChanged;
    event Action<bool>? VisibleChanged;
    event Action? Ready;

    void Close();
    void Open(string path, IReadOnlyDictionary<string, string?>? queryParameters = null);
    void SetPath(string path, bool clientSide = false);
}

internal sealed class PlayerBrowserFeature : IPlayerBrowserFeature, IDisposable
{
    public event Action<string, bool>? PathChanged;
    public event Action<bool>? DevToolsStateChanged;
    public event Action<bool>? VisibleChanged;
    public event Action? Ready;

    private readonly IBrowserGuiService _browserGuiService;
    private readonly IBrowserService _browserService;

    public bool IsReady { get; private set; }

    private string _path = "/";
    public string Path
    {
        get => _path; set
        {
            if (!IsReady)
                throw new BrowserNotReadyException();

            PathChanged?.Invoke(value, false);
            _path = value;
        }
    }

    private bool _devTools;
    public bool DevTools
    {
        get => _devTools; set
        {
            if (!IsReady)
                throw new BrowserNotReadyException();

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
            if (!IsReady)
                throw new BrowserNotReadyException();

            if (_visible != value)
            {
                _visible = value;
                VisibleChanged?.Invoke(value);
                _browserService.SetVisible(Player, value);
            }
        }
    }
    public string Key { get; init; }

    public RealmPlayer Player { get; init; }

    public PlayerBrowserFeature(IBrowserGuiService browserGuiService, IBrowserService browserService, PlayerContext playerContext)
    {
        Key = browserGuiService.GenerateKey();
        _browserGuiService = browserGuiService;
        _browserService = browserService;
        Player = playerContext.Player;
        browserService.BrowserStarted += HandleBrowserStarted;
    }

    private void HandleBrowserStarted(Player player)
    {
        if (player != Player)
            return;

        IsReady = true;
        Ready?.Invoke();

        if (_browserGuiService.AuthorizePlayer(Key, Player))
        {
            var url = $"/realmGuiIndex/?{BrowserConstants.QueryParameterName}={Key}";
            SetPath(url, true);
        }
    }

    public void SetPath(string path, bool clientSide = false)
    {
        if (!IsReady)
            throw new BrowserNotReadyException();

        PathChanged?.Invoke(path, clientSide);
        _path = path;
        if (clientSide)
            _browserService.SetPath(Player, _path);
    }

    /// <summary>
    /// Server side only
    /// </summary>
    /// <param name="path"></param>
    public void Open(string path, IReadOnlyDictionary<string, string?>? queryParameters = null)
    {
        if (!IsReady)
            throw new BrowserNotReadyException();

        if (queryParameters != null)
            path = QueryHelpers.AddQueryString(path, queryParameters);
        Path = path;
        Visible = true;
    }

    public void Close()
    {
        if (!IsReady)
            throw new BrowserNotReadyException();

        SetPath(BrowserConstants.DefaultPage);
        Visible = false;
    }

    public void Dispose()
    {
        _browserGuiService.UnauthorizePlayer(Player);
    }
}
