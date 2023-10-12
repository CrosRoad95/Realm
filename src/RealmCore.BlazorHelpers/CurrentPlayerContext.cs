using RealmCore.ECS;
using RealmCore.Server.Components.Players;
using RealmCore.Server.Components.Players.Abstractions;
using RealmCore.Server.Interfaces;
using RealmCore.Server.Services;
using System.Security.Claims;

namespace RealmCore.BlazorHelpers;

public class CurrentPlayerContext
{
    public Entity? Entity { get; }
    public IRealmServer? Server { get; }
    protected BrowserComponent BrowserComponent { get; }
    protected IBrowserGuiService BrowserGuiService { get; }
    public ClaimsPrincipal ClaimsPrincipal { get; }

    public event Action<string>? PathChanged;
    public CurrentPlayerContext(IHttpContextAccessor httpContent, IRealmServer realmServer)
    {
        Server = realmServer;
        ClaimsPrincipal = httpContent.HttpContext.User;
        if (ClaimsPrincipal.Identity != null && ClaimsPrincipal.Identity.IsAuthenticated)
        {
            var browserGuiService = realmServer.GetRequiredService<IBrowserGuiService>();
            var keyClaim = ClaimsPrincipal.Claims.Where(x => x.Type == browserGuiService.KeyName).FirstOrDefault();
            if (keyClaim == null)
                return;
            if(browserGuiService.TryGetEntityByKey(keyClaim.Value, out var entity) && entity != null)
            {
                Entity = entity;
            }
        }
    }
}

public class CurrentPlayerContext<TGuiPageComponent> : CurrentPlayerContext where TGuiPageComponent : GuiBlazorComponent
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
