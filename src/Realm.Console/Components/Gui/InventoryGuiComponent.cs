namespace Realm.Console.Components.Gui;

public sealed class InventoryGuiComponent : StatefulGuiComponent<InventoryGuiComponent.InventoryState>
{
    public class InventoryState
    {

    }

    public InventoryGuiComponent() : base("inventory", false, new InventoryState())
    {

    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        throw new NotImplementedException();
    }
}
