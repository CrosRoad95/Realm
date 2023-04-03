using Microsoft.AspNetCore.Html;

namespace Realm.Extensions.RazorGui.TagHelpers;

[HtmlTargetElement("form", Attributes = "name")]
public class FormTagHelper : TagHelper
{
    public string Name { get; set; }
    private readonly List<InputTagHelper> _inputTagHelpers = new();
    private ButtonTagHelper? _buttonTagHelper = null;
    public override void Init(TagHelperContext context)
    {
        if (context.Items.ContainsKey("current-form"))
            throw new NotSupportedException("Nested forms not supported");

        context.Items.Add("current-form", this);
    }

    public void AddInput(InputTagHelper inputTagHelper)
    {
        _inputTagHelpers.Add(inputTagHelper);
    }

    public void AddSubmitButton(ButtonTagHelper buttonTagHelper)
    {
        _buttonTagHelper = buttonTagHelper;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var childContext = await output.GetChildContentAsync();
        string originalContent = childContext.GetContent();
        output.TagName = null;
        output.Content.AppendHtml(originalContent);
        if (_buttonTagHelper == null)
            throw new Exception("No button defined.");

        if (!_inputTagHelpers.Any())
            throw new Exception("No inputs defined.");

        output.Content.AppendHtmlLine($"local {Name} = createForm(\"{Name}\", {{");
        foreach (var item in _inputTagHelpers)
        {
            output.Content.AppendHtmlLine($"\t[\"{item.Name}\"] = {item.Id},");
        }
        output.Content.AppendHtml($"}});");

        context.Items.Remove("current-form");
    }
}
