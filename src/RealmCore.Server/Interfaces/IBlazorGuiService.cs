namespace RealmCore.Server.Interfaces;

public interface IBlazorGuiService
{
    Func<BlazorGuiComponent, string, string, Task>? InvokeVoidAsync { get; set; }
    Func<BlazorGuiComponent, string, string, Task<object>>? InvokeAsync { get; set; }

    internal Task<object?> RelayInvokeAsync(BlazorGuiComponent component, string identifier, string args);
    internal Task RelayInvokeVoidAsync(BlazorGuiComponent component, string identifier, string args);
}
