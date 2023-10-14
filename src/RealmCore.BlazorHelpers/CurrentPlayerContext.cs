using RealmCore.ECS;
using RealmCore.Server.Components.Players;
using RealmCore.Server.Components.Players.Abstractions;
using RealmCore.Server.Interfaces;
using RealmCore.Server.Services;
using System.Security.Claims;

namespace RealmCore.BlazorHelpers;

public class CurrentPlayerContext
{
    public IRealmServer? Server { get; }
    protected IBrowserGuiService BrowserGuiService { get; }
    public ClaimsPrincipal ClaimsPrincipal { get; }
    public Entity? Entity { get; }
    protected BrowserComponent? BrowserComponent { get; }

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
                Entity = entity;
                BrowserComponent = entity.GetRequiredComponent<BrowserComponent>();
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
