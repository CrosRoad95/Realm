namespace RealmCore.Server.Interfaces;

public interface IBlazorGuiService
{
    Action<BlazorGuiComponent, string, string>? InvokeVoidAsync { get; set; }
    Func<BlazorGuiComponent, string, string, Task<object>>? InvokeAsync { get; set; }

    internal Task<object> RelayInvokeAsync(BlazorGuiComponent component, string identifier, string args);
    internal void RelayInvokeVoidAsync(BlazorGuiComponent component, string identifier, string args);
}
