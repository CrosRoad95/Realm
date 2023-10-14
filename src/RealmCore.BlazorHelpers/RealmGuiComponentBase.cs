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
    [Inject]
    protected NavigationManager NavigationManager { get; private set; }
    protected ClaimsPrincipal ClaimsPrincipal => CurrentPlayerContext.ClaimsPrincipal ?? throw new InvalidOperationException();

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
            CurrentPlayerContext.PathChanged += HandlePathChanged;
    }

    private void HandlePathChanged(string? path)
    {
        NavigationManager.NavigateTo(path ?? "/");
    }
}

[BlazorGuiAuthorization]
[RenderModeServer]
public class RealmGuiComponentBase<TGuiPageComponent> : ComponentBase where TGuiPageComponent : BrowserGuiComponent
{
    [Inject]
    protected CurrentPlayerContext<TGuiPageComponent> CurrentPlayerContext { get; private set; }
    [Inject]
    protected NavigationManager NavigationManager { get; private set; }
    protected ClaimsPrincipal ClaimsPrincipal => CurrentPlayerContext.ClaimsPrincipal ?? throw new InvalidOperationException();
    protected TGuiPageComponent Component => CurrentPlayerContext.Component ?? throw new InvalidOperationException();

    protected override void OnAfterRender(bool firstRender)
    {
        if(firstRender)
            CurrentPlayerContext.PathChanged += HandlePathChanged;
    }

    private void HandlePathChanged(string? path)
    {
        NavigationManager.NavigateTo(path ?? "/");
    }
}
