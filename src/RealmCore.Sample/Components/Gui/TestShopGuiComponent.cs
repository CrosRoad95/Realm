using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Sample.Components.Gui;

public sealed class TestShopGuiComponent : DxGuiComponent
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
