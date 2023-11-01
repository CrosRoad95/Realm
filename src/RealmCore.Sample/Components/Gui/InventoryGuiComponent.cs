using RealmCore.Sample.Data;
using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Sample.Components.Gui;

[ComponentUsage(false)]
public sealed class InventoryGuiComponent : StatefulDxGuiComponent<InventoryGuiComponent.InventoryState>
{
    public class InventoryState
    {
        public struct InventoryItem
        {
            public string localId;
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

    public void Attach()
    {
        var inventory = ((IComponents)Element).GetRequiredComponent<InventoryComponent>();
        inventory.ItemAdded += HandleItemAdded;
        inventory.ItemRemoved += HandleItemRemoved;
        inventory.ItemChanged += HandleItemChanged;
        base.Attach();
    }

    public void Dispose()
    {
        var inventory = ((IComponents)Element).GetRequiredComponent<InventoryComponent>();
        inventory.ItemAdded -= HandleItemAdded;
        inventory.ItemRemoved -= HandleItemRemoved;
        inventory.ItemChanged -= HandleItemChanged;
        base.Dispose();
    }

    private void HandleItemAdded(InventoryComponent inventoryComponent, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
        ChangeState(x => x.Number, (double)inventoryComponent.Number);
    }

    private void HandleItemRemoved(InventoryComponent inventoryComponent, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
        ChangeState(x => x.Number, (double)inventoryComponent.Number);
    }

    private void HandleItemChanged(InventoryComponent inventoryComponent, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
        ChangeState(x => x.Number, (double)inventoryComponent.Number);
    }

    private IEnumerable<InventoryState.InventoryItem> MapItems()
    {
        var inventory = ((IComponents)Element).GetRequiredComponent<InventoryComponent>();
        return inventory.Items.Select(x => new InventoryState.InventoryItem
        {
            localId = x.LocalId,
            id = x.ItemId,
            name = x.Name,
            number = x.Number,
            actions = (double)x.AvailableActions,
            metaData = x.MetaData
        }).OrderBy(x => x.id).ThenByDescending(x => x.number);
    }

    protected override void PreGuiOpen(InventoryState state)
    {
        var inventory = ((IComponents)Element).GetRequiredComponent<InventoryComponent>();
        state.Size = (double)inventory.Size;
        state.Number = (double)inventory.Number;
        state.Items.Clear();
        state.Items.AddRange(MapItems());
    }

    protected override async Task HandleForm(IFormContext formContext)
    {
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        switch (actionContext.ActionName)
        {
            case "doItemAction":
                var useItemData = actionContext.GetData<UseItemData>();
                var inventory = ((IComponents)Element).GetRequiredComponent<InventoryComponent>();
                if (inventory.TryGetByLocalId(useItemData.LocalId, out Item item))
                    inventory.TryUseItem(item, useItemData.ItemAction);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
