namespace RealmCore.Server.Interfaces;

public interface IBlazorGuiService
{
    Func<BrowserComponent, string, string, Task>? InvokeVoidAsync { get; set; }
    Func<BrowserComponent, string, string, Task<object>>? InvokeAsync { get; set; }

    internal Task<object?> RelayInvokeAsync(BrowserComponent component, string identifier, string args);
    internal Task RelayInvokeVoidAsync(BrowserComponent component, string identifier, string args);
}
