using Microsoft.AspNetCore.Html;

namespace Realm.Extensions.RazorGui.TagHelpers;

[HtmlTargetElement("input", Attributes = "left, top, width, height", TagStructure = TagStructure.NormalOrSelfClosing)]
[HtmlTargetElement("input", Attributes = "left, top, width, height, id", TagStructure = TagStructure.NormalOrSelfClosing)]
[HtmlTargetElement("input", Attributes = "left, top, width, height, id, name", TagStructure = TagStructure.NormalOrSelfClosing)]
[HtmlTargetElement("input", Attributes = "left, top, width, height, id, masked", TagStructure = TagStructure.NormalOrSelfClosing)]
[HtmlTargetElement("input", Attributes = "left, top, width, height, id, masked, name", TagStructure = TagStructure.NormalOrSelfClosing)]
public class InputTagHelper : TagHelper
{
    private static int _id = 0;
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Id { get; set; } = "input" + ++_id;
    public string Name { get; set; } = "";
    public bool Masked { get; set; }

    public override void Init(TagHelperContext context)
    {
        if(context.Items.TryGetValue("current-form", out object? value) && value is FormTagHelper formTagHelper)
        {
            formTagHelper.AddInput(this);
        }
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        if(Masked)
        {
            output.Content.AppendHtmlLine($"local {Id} = guiProvider.input({Left}, {Top}, {Width}, {Height}, window);");
            output.Content.AppendHtml($"guiProvider.setMasked({Id}, true)");
        }
        else
        {
            output.Content.AppendHtml($"local {Id} = guiProvider.input({Left}, {Top}, {Width}, {Height}, window);");
        }
    }
}
