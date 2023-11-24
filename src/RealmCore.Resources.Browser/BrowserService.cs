using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Browser;

internal sealed class BrowserService : IBrowserService
{
    public event Action<Player>? BrowserStarted;
    public event Action<Player>? BrowserStopped;
    public event Action<Player>? BrowserReady;

    public Action<IMessage>? MessageHandler { get; set; }


    private readonly Uri _baseUrl;

    public BrowserService(IOptions<BrowserOptions> browserOptions, ILogger<BrowserService> logger)
    {
        if(browserOptions.Value.BaseRemoteUrl != null)
        {
            try
            {
                _baseUrl = new Uri(browserOptions.Value.BaseRemoteUrl);
            }
            catch (UriFormatException ex)
            {
                logger.LogError(ex, "Invalid URI format: {baseRemoteUrl}", browserOptions.Value.BaseRemoteUrl);
                throw;
            }
        }
        else
        {
            _baseUrl = new Uri("");
        }
    }

    public void RelayBrowserReady(Player player)
    {
        BrowserReady?.Invoke(player);
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
