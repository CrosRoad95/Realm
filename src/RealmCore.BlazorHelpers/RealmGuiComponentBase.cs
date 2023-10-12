using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using RealmCore.Server.Components.Players.Abstractions;
using System.Security.Claims;

namespace RealmCore.BlazorHelpers;

[BlazorGuiAuthorization]
[RenderModeServer]
public class RealmGuiComponentBase : ComponentBase
{
    [Inject]
    protected CurrentPlayerContext CurrentPlayerContext { get; private set; }
    protected ClaimsPrincipal ClaimsPrincipal => CurrentPlayerContext.ClaimsPrincipal ?? throw new InvalidOperationException();
}

[BlazorGuiAuthorization]
[RenderModeServer]
public class RealmGuiComponentBase<TGuiPageComponent> : ComponentBase where TGuiPageComponent : GuiBlazorComponent
{
    [Inject]
    protected CurrentPlayerContext<TGuiPageComponent> CurrentPlayerContext { get; private set; }
    protected ClaimsPrincipal ClaimsPrincipal => CurrentPlayerContext.ClaimsPrincipal ?? throw new InvalidOperationException();
    protected TGuiPageComponent Component => CurrentPlayerContext.Component ?? throw new InvalidOperationException();
}
