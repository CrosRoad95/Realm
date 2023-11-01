using RealmCore.Server.Components.Players;
using RealmCore.Server.Components.Players.Abstractions;
using RealmCore.Server.Elements;
using RealmCore.Server.Interfaces;
using RealmCore.Server.Services;
using System.Security.Claims;

namespace RealmCore.BlazorHelpers;

public class CurrentPlayerContext : IDisposable
{
    private readonly RealmPlayer? _player;
    private readonly BrowserComponent? _browserComponent;
    public IRealmServer? Server { get; }
    internal IBrowserGuiService BrowserGuiService { get; }
    public ClaimsPrincipal ClaimsPrincipal { get; }
    protected BrowserComponent BrowserComponent => _browserComponent ?? throw new ArgumentNullException(nameof(BrowserComponent));
    public RealmPlayer Player => _player ?? throw new ArgumentNullException(nameof(RealmPlayer));
    public string Name => _player.Name;

    internal event Action<string?>? PathChanged;
    public CurrentPlayerContext(IHttpContextAccessor httpContent, IRealmServer realmServer)
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
                _browserComponent = player.GetRequiredComponent<BrowserComponent>();
                _browserComponent.PathChanged += HandlePathChanged;
            }
        }
    }

    private void HandlePathChanged(BrowserComponent browserComponent, string path, bool clientSide)
    {
        if(!clientSide)
            PathChanged?.Invoke(path);
    }

    public virtual void Dispose()
    {
        if(BrowserComponent != null)
        {
            BrowserComponent.PathChanged -= HandlePathChanged;
        }
    }
}

public class CurrentPlayerContext<TGuiPageComponent> : CurrentPlayerContext where TGuiPageComponent : BrowserGuiComponent, IDisposable
{
    public TGuiPageComponent Component => Player.GetRequiredComponent<TGuiPageComponent>();

    public CurrentPlayerContext(IHttpContextAccessor httpContent, IRealmServer realmServer) : base(httpContent, realmServer)
    {
    }

    public void Close()
    {
        Player.DestroyComponent(Component);
    }
}
