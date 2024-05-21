using RealmCore.Sample.Data;

namespace RealmCore.Sample.Concepts.Gui;

public sealed class InventoryGui : ReactiveDxGui<InventoryGui.InventoryState>, IGuiHandlers
{
    public class InventoryState
    {
        public struct InventoryItem
        {
            public string localId;
            public double id;
            public double number;
            public double actions;
            public object metaData;
        }

        public double Number { get; set; }
        public double Size { get; set; }
        public List<InventoryItem> Items { get; set; } = [];
    }

    private readonly Inventory _inventory;
    public InventoryGui(PlayerContext playerContext) : base(playerContext.Player, "inventory", false, new())
    {
        _inventory = Player.Inventory.Primary ?? throw new NullReferenceException();
        _inventory.ItemAdded += HandleItemAdded;
        _inventory.ItemRemoved += HandleItemRemoved;
        _inventory.ItemChanged += HandleItemChanged;
    }

    public override void Dispose()
    {
        if (Player.Inventory.TryGetPrimary(out var inventory))
        {
            inventory.ItemAdded -= HandleItemAdded;
            inventory.ItemRemoved -= HandleItemRemoved;
            inventory.ItemChanged -= HandleItemChanged;
        }
        base.Dispose();
    }

    private void HandleItemAdded(Inventory inventory, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
        ChangeState(x => x.Number, (double)inventory.Number);
    }

    private void HandleItemRemoved(Inventory inventory, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
        ChangeState(x => x.Number, (double)inventory.Number);
    }

    private void HandleItemChanged(Inventory inventory, Item item)
    {
        ChangeState(x => x.Items, MapItems().ToList());
        ChangeState(x => x.Number, (double)inventory.Number);
    }

    private IEnumerable<InventoryState.InventoryItem> MapItems()
    {
        return _inventory.Items.Select(x => new InventoryState.InventoryItem
        {
            localId = x.LocalId,
            id = x.ItemId,
            number = x.Number,
            actions = (double)x.AvailableActions,
            metaData = x.MetaData
        }).OrderBy(x => x.id).ThenByDescending(x => x.number);
    }

    protected override void PreGuiOpen(InventoryState state)
    {
        state.Size = (double)_inventory.Size;
        state.Number = (double)_inventory.Number;
        state.Items.Clear();
        state.Items.AddRange(MapItems());
    }

    public Task HandleAction(IActionContext actionContext)
    {
        switch (actionContext.ActionName)
        {
            case "doItemAction":
                var useItemData = actionContext.GetData<UseItemData>();
                if (_inventory.TryGetByLocalId(useItemData.LocalId, out Item item))
                    _inventory.TryUseItem(item, useItemData.ItemAction);
                break;
            default:
                throw new NotImplementedException();
        }

        return Task.CompletedTask;
    }

    public Task HandleForm(IFormContext formContext)
    {
        return Task.CompletedTask;
    }
}
