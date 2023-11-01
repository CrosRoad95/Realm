using RealmCore.Server.Enums;
using SlipeServer.Server.Enums;

namespace RealmCore.Sample.Logic;

public class ItemsLogic : ComponentLogic<InventoryComponent>
{
    private readonly ChatBox _chatBox;

    public ItemsLogic(ItemsRegistry itemsRegistry, IElementFactory elementFactory, ChatBox chatBox) : base(elementFactory)
    {
        itemsRegistry.Add(1, new ItemRegistryEntry
        {
            Size = 1,
            StackSize = 4,
            Name = "Test item id 1",
            AvailableActions = ItemAction.Use,
        });
        itemsRegistry.Add(2, new ItemRegistryEntry
        {
            Size = 2,
            StackSize = 4,
            Name = "Foo item id 2",
            AvailableActions = ItemAction.Use,
        });
        itemsRegistry.Add(3, new ItemRegistryEntry
        {
            Size = 1,
            StackSize = 1,
            Name = "Sample weapon",
            AvailableActions = ItemAction.None,
        });
        itemsRegistry.Add(4, new ItemRegistryEntry
        {
            Size = 0.05m,
            StackSize = 10,
            Name = "item size 0.05",
            AvailableActions = ItemAction.None,
        });

        _chatBox = chatBox;
    }

    protected override void ComponentAdded(InventoryComponent inventoryComponent)
    {
        inventoryComponent.ItemAdded += HandleInventoryComponentItemAdded;
        inventoryComponent.ItemRemoved += HandleInventoryComponentItemRemoved;
    }

    protected override void ComponentDetached(InventoryComponent inventoryComponent)
    {
        inventoryComponent.ItemAdded -= HandleInventoryComponentItemAdded;
        inventoryComponent.ItemRemoved -= HandleInventoryComponentItemRemoved;
    }

    private void HandleInventoryComponentItemRemoved(InventoryComponent inventoryComponent, Item item)
    {
        var itemId = item.ItemId;
        switch (itemId)
        {
            case 3:
                if (!inventoryComponent.HasItemById(itemId))
                {
                    var player = (RealmPlayer)inventoryComponent.Element;
                    player.Weapons.Add(new SlipeServer.Server.Elements.Structs.Weapon(WeaponId.Bat, 1));
                    _chatBox.OutputTo(player, "Bat taken");
                }
                break;
        }
    }

    private void HandleInventoryComponentItemAdded(InventoryComponent inventoryComponent, Item item)
    {
        var itemId = item.ItemId;
        switch (itemId)
        {
            case 3:
                var player = (RealmPlayer)inventoryComponent.Element;
                if (!player.Weapons.Any(x => x.Type == WeaponId.Bat && x.Ammo > 0))
                {
                    player.Weapons.Add(new SlipeServer.Server.Elements.Structs.Weapon(WeaponId.Bat, 1));
                    _chatBox.OutputTo(player, "Bat taken");
                }
                break;
        }
    }

    private Task Use(InventoryComponent inventoryComponent, Item item, ItemAction action)
    {
        switch (action)
        {
            case ItemAction.Use:
                _chatBox.OutputTo((RealmPlayer)inventoryComponent.Element, $"Item used: {item.Name}");
                inventoryComponent.RemoveItem(item);
                break;
        }
        return Task.CompletedTask;
    }
}
