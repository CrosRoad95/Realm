namespace Realm.Extensions.RazorGui.TagHelpers;

[HtmlTargetElement("checkbox", Attributes = "text, left, top, width, height", TagStructure = TagStructure.NormalOrSelfClosing)]
[HtmlTargetElement("checkbox", Attributes = "text, left, top, width, height, id", TagStructure = TagStructure.NormalOrSelfClosing)]
public class CheckboxTagHelper : TagHelper
{
    public string Text { get; set; }
    public int Left { get; set; }
    public int Top { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Id { get; set; } = "_";

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
        output.Content.AppendHtml($"\tlocal {Id} = guiProvider.checkbox(\"{Text}\", {Left}, {Top}, {Width}, {Height}, false, window);");
    }
}
