using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealmCore.Resources.Base.Interfaces;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;

namespace RealmCore.Resources.CEFBlazorGui;

internal sealed class CEFBlazorGuiService : ICEFBlazorGuiService
{
    public event Action<Player>? PlayerCEFBlazorGuiStarted;
    public event Action<Player>? PlayerCEFBlazorGuiStopped;
    private readonly IElementCollection _elementCollection;
    private readonly ILogger<CEFBlazorGuiService> _logger;

    public Action<IMessage>? MessageHandler { get; set; }

    public Action<Player>? RelayPlayerBrowserReady { get; set; }
    public Action<Player>? RelayPlayerBlazorReady { get; set; }

    private readonly Uri? _baseUrl;

    public CEFBlazorGuiService(IOptions<BrowserOptions> browserOptions, IElementCollection elementCollection, ILogger<CEFBlazorGuiService> logger)
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
        _elementCollection = elementCollection;
        _logger = logger;
        if (browserOptions.Value.DebuggingServer)
        {
            // TODO:
        }
    }

    public void HandlePlayerBrowserReady(Player player)
    {
        RelayPlayerBrowserReady?.Invoke(player);
    }

    public void HandlePlayerBlazorReady(Player player)
    {
        RelayPlayerBlazorReady?.Invoke(player);
    }

    public void ToggleDevTools(Player player, bool enabled)
    {
        MessageHandler?.Invoke(new ToggleDevToolsMessage(player, enabled));
    }

    public void SetVisible(Player player, bool visible)
    {
        MessageHandler?.Invoke(new SetVisibleMessage(player, visible));
    }

    public void SetPath(Player player, string path, bool force, bool isAsync)
    {
        MessageHandler?.Invoke(new SetPathMessage(player, path, force, isAsync));
    }

    public void SetRemotePath(Player player, string path)
    {
        if (_baseUrl == null)
            throw new Exception("Failed to navigate to remote path without base url");
        var uri = new Uri(_baseUrl, path).ToString();
        MessageHandler?.Invoke(new SetRemotePathMessage(player, uri));
    }
}
