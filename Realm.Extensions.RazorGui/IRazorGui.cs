namespace Realm.Extensions.RazorGui;

public interface IRazorGui
{
    Task<string> RenderGui<T>(string guiName, string viewName, T model, Dictionary<string, object>? viewDataOrViewBag = null);
}
