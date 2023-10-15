using RealmCore.ECS;
using RealmCore.Server.Components.Elements;
using RealmCore.Server.Components.Players;
using RealmCore.Server.Components.Players.Abstractions;
using RealmCore.Server.Interfaces;
using RealmCore.Server.Services;
using System.Security.Claims;

namespace RealmCore.BlazorHelpers;

public class CurrentPlayerContext : IDisposable
{
    private readonly Entity? _entity;
    private readonly BrowserComponent? _browserComponent;
    public IRealmServer? Server { get; }
    protected IBrowserGuiService BrowserGuiService { get; }
    public ClaimsPrincipal ClaimsPrincipal { get; }
    protected BrowserComponent BrowserComponent => _browserComponent ?? throw new ArgumentNullException(nameof(BrowserComponent));
    public Entity Entity => _entity ?? throw new ArgumentNullException(nameof(Entity));
    public string Name => Entity.GetRequiredComponent<PlayerElementComponent>().Name;

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
            if(BrowserGuiService.TryGetEntityByKey(keyClaim.Value, out var entity) && entity != null)
            {
                _entity = entity;
                _browserComponent = entity.GetRequiredComponent<BrowserComponent>();
                _browserComponent.PathChanged += HandlePathChanged;
            }
        }
    }

    private void HandlePathChanged(BrowserComponent browserComponent, string path, bool clientSide)
    {
        if(!clientSide)
            PathChanged?.Invoke(path);
    }

    public void Dispose()
    {
        if(BrowserComponent != null)
        {
            BrowserComponent.PathChanged -= HandlePathChanged;
        }
    }
}

public class CurrentPlayerContext<TGuiPageComponent> : CurrentPlayerContext where TGuiPageComponent : BrowserGuiComponent
{
    public TGuiPageComponent Component { get; }

    public CurrentPlayerContext(IHttpContextAccessor httpContent, IRealmServer realmServer) : base(httpContent, realmServer)
    {
        Component = Entity.GetRequiredComponent<TGuiPageComponent>();
    }

    public void Close()
    {
        Component.Entity.DestroyComponent(Component);
    }
}
