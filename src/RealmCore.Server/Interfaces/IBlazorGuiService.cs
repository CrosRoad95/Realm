namespace RealmCore.Server.Interfaces;

public interface IBlazorGuiService
{
    Func<BrowserGuiComponent, string, string, Task>? InvokeVoidAsync { get; set; }
    Func<BrowserGuiComponent, string, string, Task<object>>? InvokeAsync { get; set; }

    internal Task<object?> RelayInvokeAsync(BrowserGuiComponent component, string identifier, string args);
    internal Task RelayInvokeVoidAsync(BrowserGuiComponent component, string identifier, string args);
}
