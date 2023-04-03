using Microsoft.AspNetCore.Html;

namespace Realm.Extensions.RazorGui.TagHelpers;

[HtmlTargetElement("button", Attributes = "text, left, top, width, height", TagStructure = TagStructure.NormalOrSelfClosing)]
[HtmlTargetElement("button", Attributes = "text, left, top, width, height, id", TagStructure = TagStructure.NormalOrSelfClosing)]
[HtmlTargetElement("button", Attributes = "text, left, top, width, height, luaFunction", TagStructure = TagStructure.NormalOrSelfClosing)]
[HtmlTargetElement("button", Attributes = "text, left, top, width, height, id, luaFunction", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ButtonTagHelper : TagHelper
{
    private static int _id = 0;
    public string Text { get; set; }
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Id { get; set; } = "button" + ++_id;
    public string LuaFunction { get; set; }

    public override void Init(TagHelperContext context)
    {
        if (context.Items.TryGetValue("current-form", out object? value) && value is FormTagHelper formTagHelper)
        {
            formTagHelper.AddSubmitButton(this);
        }
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.AppendHtmlLine($"local {Id} = guiProvider.button(\"{Text}\", {Left}, {Top}, {Width}, {Height}, window);");
        if(LuaFunction != null)
        {
            output.Content.AppendHtml($"guiProvider.onClick({Id}, {LuaFunction});");
        }
    }
}
