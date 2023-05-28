namespace RealmCore.Server.Interfaces;

public interface IBlazorGuiService
{
    event Action<BlazorGuiComponent, string, string>? InvokeVoidAsync;

    internal void RelayInvokeVoidAsync(BlazorGuiComponent component, string identifier, string args);
}
