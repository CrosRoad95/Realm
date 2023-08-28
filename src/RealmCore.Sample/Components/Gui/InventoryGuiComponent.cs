using RealmCore.Server.Contexts.Interfaces;

namespace RealmCore.Console.Components.Gui;

[ComponentUsage(false)]
public sealed class InventoryGuiComponent : StatefulGuiComponent<InventoryGuiComponent.InventoryState>
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

    protected override void Attach()
    {
        var inventory = Entity.GetRequiredComponent<InventoryComponent>();
        inventory.ItemAdded += HandleItemAdded;
        inventory.ItemRemoved += HandleItemRemoved;
        inventory.ItemChanged += HandleItemChanged;
        base.Attach();
    }

    public override void Dispose()
    {
        var inventory = Entity.GetRequiredComponent<InventoryComponent>();
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
        var inventory = Entity.GetRequiredComponent<InventoryComponent>();
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
        ThrowIfDisposed();

        var inventory = Entity.GetRequiredComponent<InventoryComponent>();
        state.Size = (double)inventory.Size;
        state.Number = (double)inventory.Number;
        state.Items.Clear();
        state.Items.AddRange(MapItems());
    }

    protected override async Task HandleForm(IFormContext formContext)
    {
        ThrowIfDisposed();
    }

    protected override async Task HandleAction(IActionContext actionContext)
    {
        ThrowIfDisposed();
        switch (actionContext.ActionName)
        {
            case "doItemAction":
                var useItemData = actionContext.GetData<UseItemData>();
                var inventory = Entity.GetRequiredComponent<InventoryComponent>();
                if (inventory.TryGetByLocalId(useItemData.LocalId, out Item item))
                    inventory.TryUseItem(item, useItemData.ItemAction);
                break;
            default:
                throw new NotImplementedException();
        }
    }
}
