using RealmCore.Server.Concepts.Gui;

namespace RealmCore.BlazorHelpers;

public class CurrentPlayerContext : IDisposable
{
    private readonly RealmPlayer? _player;
    private readonly IPlayerBrowserService? _browserService;
    public RealmServer? Server { get; }
    internal IBrowserGuiService BrowserGuiService { get; }
    public ClaimsPrincipal ClaimsPrincipal { get; }
    protected IPlayerBrowserService BrowserService => _browserService ?? throw new ArgumentNullException(nameof(BrowserService));
    public RealmPlayer Player => _player ?? throw new ArgumentNullException(nameof(RealmPlayer));
    public string Name => Player.Name;

    internal event Action<string?>? PathChanged;
    public CurrentPlayerContext(IHttpContextAccessor httpContent, RealmServer realmServer)
    {
        Server = realmServer;
        if(httpContent.HttpContext == null)
            throw new ArgumentNullException(nameof(httpContent.HttpContext));

        ClaimsPrincipal = httpContent.HttpContext.User;
        BrowserGuiService = realmServer.GetRequiredService<IBrowserGuiService>();
        if (ClaimsPrincipal.Identity != null && ClaimsPrincipal.Identity.IsAuthenticated)
        {
            var keyClaim = ClaimsPrincipal.Claims.Where(x => x.Type == BrowserGuiService.KeyName).FirstOrDefault();
            if (keyClaim == null)
                return;
            if(BrowserGuiService.TryGetPlayerByKey(keyClaim.Value, out var player) && player != null)
            {
                _player = player;
                _browserService = player.Browser;
                _browserService.PathChanged += HandlePathChanged;
            }
        }
    }

    private void HandlePathChanged(string path, bool clientSide)
    {
        if(!clientSide)
            PathChanged?.Invoke(path);
    }

    public virtual void Dispose()
    {
        if(BrowserService != null)
        {
            BrowserService.PathChanged -= HandlePathChanged;
        }
    }
}

public class CurrentPlayerContext<TGui> : CurrentPlayerContext where TGui : BrowserGui, IDisposable
{
    public TGui Gui => (TGui)Player.Gui.Current;

    public CurrentPlayerContext(IHttpContextAccessor httpContent, RealmServer realmServer) : base(httpContent, realmServer)
    {
    }

    public void Close()
    {
        Player.Gui.Current = null;
    }
}
