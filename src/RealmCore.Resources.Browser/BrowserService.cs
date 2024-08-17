using RealmCore.Resources.Base.Interfaces;
using System.Numerics;

namespace RealmCore.Resources.Browser;

public interface IBrowserService
{
    event Action<Player>? BrowserStarted;
    event Action<Player>? BrowserStopped;
    event Action<Player>? BrowserLoaded;

    Action<IMessage>? MessageHandler { get; set; }
    HttpClient HttpClient { get; }

    internal void RelayBrowserReady(Player player);

    void ToggleDevTools(Player player, bool enabled);
    void SetVisible(Player player, bool visible);
    void SetPath(Player player, string path);
    void RelayBrowserStarted(Player player);
    void Load(Player player, Vector2 screenSize);
}

internal sealed class BrowserService : IBrowserService
{
    public event Action<Player>? BrowserStarted;
    public event Action<Player>? BrowserStopped;
    public event Action<Player>? BrowserLoaded;

    public Action<IMessage>? MessageHandler { get; set; }

    private readonly Uri _baseUrl;
    private readonly IOptions<BrowserOptions> _browserOptions;
    private readonly HttpClient _httpClient;

    public HttpClient HttpClient => _httpClient;

    public BrowserService(IOptions<BrowserOptions> browserOptions, ILogger<BrowserService> logger)
    {
        _browserOptions = browserOptions;
        if (browserOptions.Value.BaseRemoteUrl != null)
        {
            try
            {
                _baseUrl = new Uri(browserOptions.Value.BaseRemoteUrl);
                _httpClient = new HttpClient()
                {
                    BaseAddress = _baseUrl,
                };
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, "Invalid URI format: {baseRemoteUrl}", browserOptions.Value.BaseRemoteUrl);
                // TODO:
                _baseUrl = null;
                _httpClient = null;
                throw;
            }
        }
        else
        {
            throw new NullReferenceException("Browser BaseRemoteUrl is null");
        }
    }

    private Vector2 GetBrowserSize(Vector2 screenSize)
    {
        int largeThresholdWidth = 1920;
        int mediumThresholdWidth = 1280;

        if (screenSize.X >= largeThresholdWidth)
        {
            return new Vector2(1366, 768);
        }
        else if (screenSize.X >= mediumThresholdWidth)
        {
            return new Vector2(1024, 600);
        }
        else
        {
            return new Vector2(800, 600);
        }
    }

    public void Load(Player player, Vector2 screenSize)
    {
        var browserSize = GetBrowserSize(screenSize);
        MessageHandler?.Invoke(new LoadBrowser(player, browserSize, _browserOptions.Value.BaseRemoteUrl, _browserOptions.Value.RequestWhitelistUrl));
    }

    public void RelayBrowserReady(Player player)
    {
        BrowserLoaded?.Invoke(player);
    }
    
    public void RelayBrowserStarted(Player player)
    {
        BrowserStarted?.Invoke(player);
    }

    public void ToggleDevTools(Player player, bool enabled)
    {
        MessageHandler?.Invoke(new ToggleDevToolsMessage(player, enabled));
    }

    public void SetVisible(Player player, bool visible)
    {
        MessageHandler?.Invoke(new SetVisibleMessage(player, visible));
    }

    public void SetPath(Player player, string path)
    {
        var fullUrl = new Uri(_baseUrl, path).ToString();
        MessageHandler?.Invoke(new SetPathMessage(player, fullUrl));
    }
}
