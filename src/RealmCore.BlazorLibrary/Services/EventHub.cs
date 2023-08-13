using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace RealmCore.BlazorLibrary.Services;

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
        if (_navigationManager.Uri.Contains("localhost"))
        {
            await _jsRuntime.InvokeVoidAsync("mtaTriggerEventDebug", "invokeVoidAsync", identifier, args);
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("mtaTriggerEvent", "invokeVoidAsync", identifier, args);
        }
    }

    public async ValueTask<T> InvokeAsync<T>(string identifier, params object?[]? args)
    {
        string data;
        if (_navigationManager.Uri.Contains("localhost"))
        {
            data = await _jsRuntime.InvokeAsync<string>("mtaTriggerEventWithResultDebug", "invokeAsync", identifier, args);
        }
        else
        {
            data = await _jsRuntime.InvokeAsync<string>("mtaTriggerEventWithResult", "invokeAsync", identifier, args);
        }
        return JsonSerializer.Deserialize<T>(data);
    }

    public ValueTask LocationChanged(string location)
    {
        return InvokeVoidAsync("_locationChanged", location);
    }

    public ValueTask NotifyPageReady()
    {
        return InvokeVoidAsync("_pageReady");
    }
}
