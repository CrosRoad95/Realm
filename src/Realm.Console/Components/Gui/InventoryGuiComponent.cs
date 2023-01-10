namespace Realm.Console.Components.Gui;

public sealed class InventoryGuiComponent : StatefulGuiComponent<InventoryGuiComponent.InventoryState>
{
    public class InventoryState
    {
        public struct InventoryItem
        {
            public string name;
            public double number;
            public object metaData;
        }

        public double Size { get; set; }
        public List<InventoryItem> Items { get; } = new();
    }

    public InventoryGuiComponent() : base("inventory", false, new())
    {

    }

    protected override void PreGuiOpen(InventoryState state)
    {
        var inventory = Entity.GetRequiredComponent<InventoryComponent>();
        state.Size = inventory.Size;
        state.Items.Clear();
        state.Items.AddRange(inventory.Items.Select(x => new InventoryState.InventoryItem
        {
            name = x.Name,
            number = x.Number,
            metaData = x.MetaData
        }));
    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {

    }
}
