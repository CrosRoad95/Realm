namespace RealmCore.Server.Components.Players.Abstractions;

public abstract class GuiBlazorComponent : GuiComponent
{
    public string Path { get; }

    public event Action<GuiBlazorComponent>? Changed;
    public event Action<GuiBlazorComponent, GuiBlazorComponent>? NavigationRequested;

    public GuiBlazorComponent(string path)
    {
        Path = path;
        //Path = path;
    }

    protected void StateHasChanged()
    {
        Changed?.Invoke(this);
    }

    protected void NavigateTo(GuiBlazorComponent targetBrowserGuiPageComponent)
    {
        NavigationRequested?.Invoke(this, targetBrowserGuiPageComponent);
    }
}