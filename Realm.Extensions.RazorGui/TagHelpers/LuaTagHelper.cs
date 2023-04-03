namespace Realm.Extensions.RazorGui.TagHelpers;

[HtmlTargetElement("lua", Attributes = "")]
public class LuaTagHelper : TagHelper
{
    public override void Init(TagHelperContext context)
    {
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;
    }
}
