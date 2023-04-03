namespace Realm.Extensions.RazorGui.TagHelpers;

[HtmlTargetElement("window", Attributes = "title, width, height")]
[HtmlTargetElement("window", Attributes = "title, width, height, center")]
public class WindowTagHelper : TagHelper
{
    public string Title { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Center { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();
        string originalContent = content.GetContent();

        output.TagName = null;
        output.Content.AppendHtml($"local window = guiProvider.window(\"{Title}\", 0, 0, {Width}, {Height});\n");
        if(Center)
        {
            output.Content.AppendHtml("guiProvider.centerWindow(window);\n");
        }
        output.Content.AppendHtml(originalContent);
    }
}
