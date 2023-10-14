using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.Browser;

internal sealed class BrowserService : IBrowserService
{
    public event Action<Player>? PlayerBrowserStarted;
    public event Action<Player>? PlayerBrowserStopped;

    public Action<IMessage>? MessageHandler { get; set; }

    public Action<Player>? RelayPlayerBrowserReady { get; set; }

    private readonly Uri? _baseUrl;

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
    }

    public void HandlePlayerBrowserReady(Player player)
    {
        RelayPlayerBrowserReady?.Invoke(player);
    }

    public void ToggleDevTools(Player player, bool enabled)
    {
        MessageHandler?.Invoke(new ToggleDevToolsMessage(player, enabled));
    }

    public void SetVisible(Player player, bool visible)
    {
        MessageHandler?.Invoke(new SetVisibleMessage(player, visible));
    }

    public void SetPath(Player player, string path, bool force)
    {
        MessageHandler?.Invoke(new SetPathMessage(player, path, force));
    }
}
