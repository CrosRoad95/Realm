namespace RealmCore.Server.Services;

internal class BlazorGuiService : IBlazorGuiService
{
    public Action<BlazorGuiComponent, string, string>? InvokeVoidAsync { get; set; }
    public Func<BlazorGuiComponent, string, string, Task<object>>? InvokeAsync { get; set; }

    public BlazorGuiService()
    {

    }

    public void RelayInvokeVoidAsync(BlazorGuiComponent component, string identifier, string args)
    {
        switch (identifier)
        {
            case "_locationChanged":
                var path = JsonConvert.DeserializeObject<string[]>(args)!.First();
                component.SetPath(path);
                break;
            default:
                InvokeVoidAsync?.Invoke(component, identifier, args);
                break;
        }
    }

    public Task<object> RelayInvokeAsync(BlazorGuiComponent component, string identifier, string args)
    {
        return InvokeAsync?.Invoke(component, identifier, args);
    }
}
