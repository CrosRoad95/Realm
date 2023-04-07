using Realm.Server.Contexts.Interfaces;

namespace Realm.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class TestShopGuiComponent : GuiComponent
{
    public TestShopGuiComponent() : base("shop", true)
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
    }
}
