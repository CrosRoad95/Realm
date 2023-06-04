using RealmCore.BlazorCEFGui.Extensions;
using System.Text.Json;

namespace RealmCore.BlazorCEFGui.Services;

public class RememberForm
{
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;

    public RememberForm(IJSRuntime jsRuntime, NavigationManager navigationManager)
    {
        _jsRuntime = jsRuntime;
        _navigationManager = navigationManager;
    }

    public async ValueTask Remember(string formName, object formData)
    {
        if (_navigationManager.IsDev())
        {
            await _jsRuntime.InvokeVoidAsync("mtaTriggerEventWithResultDebug", "rememberFrom", formName, formData);
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("mtaTriggerEventWithResult", "rememberFrom", formName, formData);
        }
    }

    public async ValueTask<T?> Get<T>(string formName)
    {
        string data;
        if (_navigationManager.IsDev())
        {
            data = await _jsRuntime.InvokeAsync<string>("mtaTriggerEventWithResultDebug", "getRememberFrom", formName);
        }
        else
        {
            data = await _jsRuntime.InvokeAsync<string>("mtaTriggerEventWithResult", "getRememberFrom", formName);
        }

        return JsonSerializer.Deserialize<T>(data);
    }

    public async ValueTask Forget(string formName)
    {
        if (_navigationManager.IsDev())
        {
            await _jsRuntime.InvokeVoidAsync("mtaTriggerEventWithResultDebug", "forgetFrom", formName);
        }
        else
        {
            await _jsRuntime.InvokeVoidAsync("mtaTriggerEventWithResult", "forgetFrom", formName);
        }
    }
}
