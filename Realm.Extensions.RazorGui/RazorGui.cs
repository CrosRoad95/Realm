using Razor.Templating.Core;
using System.Text;

namespace Realm.Extensions.RazorGui;

public class RazorGui : IRazorGui
{
    private readonly IRazorTemplateEngine _razorTemplateEngine;

    public RazorGui(IRazorTemplateEngine razorTemplateEngine)
    {
        _razorTemplateEngine = razorTemplateEngine;
    }

    public async Task<string> RenderGui<T>(string guiName, string viewName, T model, Dictionary<string, object>? viewDataOrViewBag = null)
    {
        var sb = new StringBuilder();
        sb.Append("local function createGui(guiProvider, state)");
        var html = await _razorTemplateEngine.RenderAsync(viewName, model, viewDataOrViewBag);
        sb.AppendLine(html);
        sb.AppendLine($"end\n");
        sb.AppendLine($"addEventHandler(\"onClientResourceStart\", resourceRoot, function()\n\tregisterGui(createGui, \"{guiName}\")\nend)");
        return sb.ToString();
    }
}