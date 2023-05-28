namespace RealmCore.BlazorCEFGui.Services;

public class EventHub
{
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;

    public EventHub(IJSRuntime jsRuntime, NavigationManager navigationManager)
    {
        _jsRuntime = jsRuntime;
        _navigationManager = navigationManager;
    }

    public async ValueTask InvokeVoidAsync(string identifier, params object?[]? args)
    {
        if(_navigationManager.Uri.Contains("localhost"))
        {
            Console.WriteLine("call 1");
            await _jsRuntime.InvokeVoidAsync("mtaTriggerEventDebug", "invokeVoidAsync", identifier, args);
        }
        else
        {
            Console.WriteLine("call 2");
            await _jsRuntime.InvokeVoidAsync("mtaTriggerEvent", "invokeVoidAsync", identifier, args);
        }
    }

    public ValueTask LocationChanged(string location)
    {
        Console.WriteLine("location {0}", location);
        return InvokeVoidAsync("_locationChanged", location);
    }
}
