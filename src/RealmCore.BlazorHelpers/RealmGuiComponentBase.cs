using Microsoft.AspNetCore.Components;

namespace RealmCore.BlazorHelpers;

[BlazorGuiAuthorization]
public class RealmGuiComponentBase : ComponentBase
{
    [Inject]
    protected CurrentPlayerContext CurrentPlayerContext { get; private set; } = default!;
    [Inject]
    protected NavigationManager NavigationManager { get; private set; } = default!;
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
public class RealmGuiComponentBase<TGui> : ComponentBase where TGui : BrowserGui
{
    [Inject]
    protected CurrentPlayerContext<BrowserGui> CurrentPlayerContext { get; private set; } = default!;
    [Inject]
    protected NavigationManager NavigationManager { get; private set; } = default!;
    protected ClaimsPrincipal ClaimsPrincipal => CurrentPlayerContext.ClaimsPrincipal ?? throw new InvalidOperationException();
    protected TGui PlayerGui => (TGui)CurrentPlayerContext.Gui;

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
