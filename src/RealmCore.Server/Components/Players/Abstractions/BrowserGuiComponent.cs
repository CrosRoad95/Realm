namespace RealmCore.Server.Components.Players.Abstractions;

public abstract class BrowserGuiComponent : GuiComponent
{
    public string Path { get; }

    public event Action<BrowserGuiComponent>? Changed;
    public event Action<BrowserGuiComponent, BrowserGuiComponent>? NavigationRequested;

    public BrowserGuiComponent(string path)
    {
        Path = path;
        //Path = path;
    }

    protected void StateHasChanged()
    {
        Changed?.Invoke(this);
    }

    protected void NavigateTo(BrowserGuiComponent targetBrowserGuiPageComponent)
    {
        NavigationRequested?.Invoke(this, targetBrowserGuiPageComponent);
    }
}