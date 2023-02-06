namespace Realm.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class TestWindowComponent : GuiComponent
{
    public TestWindowComponent() : base("test", true)
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
    }
}
