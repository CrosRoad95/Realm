namespace RealmCore.Server.Services;

internal class BlazorGuiService : IBlazorGuiService
{
    public event Action<BlazorGuiComponent, string, string>? InvokeVoidAsync;

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
}
