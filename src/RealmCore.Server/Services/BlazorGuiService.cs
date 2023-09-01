namespace RealmCore.Server.Services;

internal sealed class BlazorGuiService : IBlazorGuiService
{
    public Func<BrowserComponent, string, string, Task>? InvokeVoidAsync { get; set; }
    public Func<BrowserComponent, string, string, Task<object?>>? InvokeAsync { get; set; }

    public async Task RelayInvokeVoidAsync(BrowserComponent component, string identifier, string args)
    {
        switch (identifier)
        {
            case "_locationChanged":
                {
                    // TODO: make it better
                    string path;
                    try
                    {
                        path = JsonConvert.DeserializeObject<string>(args);
                    }
                    catch(Exception)
                    {
                        path = JsonConvert.DeserializeObject<string[]>(args).First();
                    }
                    component.InternalSetPath(path);
                }
                break;
            default:
                if (InvokeVoidAsync == null)
                    return;
                await InvokeVoidAsync.Invoke(component, identifier, args);
                break;
        }
    } 

    public Task<object?> RelayInvokeAsync(BrowserComponent component, string identifier, string args)
    {
        if(InvokeAsync != null)
            return InvokeAsync(component, identifier, args);
        return Task.FromResult<object?>(null);
    }
}
