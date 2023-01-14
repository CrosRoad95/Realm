using Realm.Domain.Inventory;

namespace Realm.Console.Components.Gui;

public sealed class InventoryGuiComponent : StatefulGuiComponent<InventoryGuiComponent.InventoryState>
{
    public class InventoryState
    {
        public struct InventoryItem
        {
            public double id;
            public string name;
            public double number;
            public double actions;
            public object metaData;
        }

        public double Number { get; set; }
        public double Size { get; set; }
        public List<InventoryItem> Items { get; set; } = new();
    }

    public InventoryGuiComponent() : base("inventory", false, new())
    {

    }

    public override Task Load()
    {
        var inventory = Entity.GetRequiredComponent<InventoryComponent>();
        inventory.ItemAdded += HandleItemAdded;
        inventory.ItemRemoved += HandleItemRemoved;
        inventory.ItemChanged += HandleItemChanged;
        return base.Load();
    }

    private void HandleItemAdded(InventoryComponent inventoryComponent, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
    }
    
    private void HandleItemRemoved(InventoryComponent inventoryComponent, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
    }
    
    private void HandleItemChanged(InventoryComponent inventoryComponent, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
        ChangeState(x => x.Number, inventoryComponent.Number);
    }

    private IEnumerable<InventoryState.InventoryItem> MapItems()
    {
        var inventory = Entity.GetRequiredComponent<InventoryComponent>();
        return inventory.Items.Select(x => new InventoryState.InventoryItem
        {
            id = x.ItemId,
            name = x.Name,
            number = x.Number,
            actions = (double)x.AvailiableActions,
            metaData = x.MetaData
        });
    }

    protected override void PreGuiOpen(InventoryState state)
    {
        var inventory = Entity.GetRequiredComponent<InventoryComponent>();
        state.Size = inventory.Size;
        state.Number = inventory.Number;
        state.Items.Clear();
        state.Items.AddRange(MapItems());
    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        switch(actionContext.ActionName)
        {
            case "use":
                var useItemData = actionContext.GetData<UseItemData>();
                var inventory = Entity.GetRequiredComponent<InventoryComponent>();
                if(await inventory.TryUseItem(useItemData.ItemId, ItemRegistryEntry.ItemAction.DefaultUse))
                {
                    ;
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
