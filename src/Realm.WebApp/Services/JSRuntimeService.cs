using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Realm.WebApp.Services;

public class JSRuntimeService
{
    private readonly IJSRuntime _jSRuntime;

    public JSRuntimeService(IJSRuntime jSRuntime)
    {
        _jSRuntime = jSRuntime;
    }

    public void ScrollToEnd(ElementReference elementReference)
    {
        _jSRuntime.InvokeVoidAsync("scrollToEnd", new object[] { elementReference });
    }

    public void ScrollToEnd(string id)
    {
        _jSRuntime.InvokeVoidAsync("scrollToEndById", new object[] { id });
    }

    public void FocusElement(string id)
    {
        _jSRuntime.InvokeVoidAsync("focusElementById", new object[] { id });
    }
}
