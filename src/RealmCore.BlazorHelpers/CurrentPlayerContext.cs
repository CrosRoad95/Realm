namespace RealmCore.BlazorHelpers;

public class CurrentPlayerContext : IDisposable
{
    private readonly RealmPlayer? _player;
    private readonly IPlayerBrowserService? _browserComponent;
    public RealmServer? Server { get; }
    internal IBrowserGuiService BrowserGuiService { get; }
    public ClaimsPrincipal ClaimsPrincipal { get; }
    protected IPlayerBrowserService BrowserService => _browserComponent ?? throw new ArgumentNullException(nameof(BrowserService));
    public RealmPlayer Player => _player ?? throw new ArgumentNullException(nameof(RealmPlayer));
    public string Name => Player.Name;

    internal event Action<string?>? PathChanged;
    public CurrentPlayerContext(IHttpContextAccessor httpContent, RealmServer realmServer)
    {
        Server = realmServer;
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
                _browserComponent = player.GetRequiredService<IPlayerBrowserService>();
                _browserComponent.PathChanged += HandlePathChanged;
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

public class CurrentPlayerContext<TGuiPageComponent> : CurrentPlayerContext where TGuiPageComponent : BrowserGuiComponent, IDisposable
{
    public TGuiPageComponent Component => Player.GetRequiredComponent<TGuiPageComponent>();

    public CurrentPlayerContext(IHttpContextAccessor httpContent, RealmServer realmServer) : base(httpContent, realmServer)
    {
    }

    public void Close()
    {
        Player.DestroyComponent(Component);
    }
}
