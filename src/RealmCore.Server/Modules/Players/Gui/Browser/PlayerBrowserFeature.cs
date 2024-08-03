namespace RealmCore.Server.Modules.Players.Gui.Browser;

public sealed class PlayerBrowserFeature : IPlayerFeature, IDisposable
{
    public event Action<PlayerBrowserFeature, string>? PathChanged;
    public event Action<PlayerBrowserFeature, bool>? DevToolsStateChanged;
    public event Action<PlayerBrowserFeature, bool>? VisibleChanged;
    public event Action<PlayerBrowserFeature>? Ready;

    private readonly BrowserGuiService _browserGuiService;
    private readonly IBrowserService _browserService;

    public bool IsReady { get; private set; }

    private string _path = "/";
    public string Path
    {
        get => _path; set
        {
            if (!IsReady)
                throw new BrowserNotReadyException();

            PathChanged?.Invoke(this, value);
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
                DevToolsStateChanged?.Invoke(this, value);
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
                VisibleChanged?.Invoke(this, value);
                _browserService.SetVisible(Player, value);
            }
        }
    }
    public string Key { get; init; }

    public RealmPlayer Player { get; init; }

    public PlayerBrowserFeature(PlayerContext playerContext, BrowserGuiService browserGuiService, IBrowserService browserService)
    {
        Key = browserGuiService.GenerateKey();
        _browserGuiService = browserGuiService;
        _browserService = browserService;
        Player = playerContext.Player;
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

    public bool TryClose()
    {
        if (!IsReady)
            throw new BrowserNotReadyException();

        if (!Visible)
            return false;

        Path = BrowserConstants.DefaultPage;
        Visible = false;
        return true;
    }

    public void RelayReady()
    {
        if (IsReady)
            throw new InvalidOperationException();

        IsReady = true;

        Ready?.Invoke(this);
    }

    public void Dispose()
    {
        _browserGuiService.UnauthorizePlayer(Player);
    }
}
