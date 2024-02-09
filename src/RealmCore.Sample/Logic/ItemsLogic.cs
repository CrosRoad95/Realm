namespace RealmCore.Sample.Logic;

public class ItemsLogic : PlayerLogic
{
    private readonly ChatBox _chatBox;

    public ItemsLogic(MtaServer mtaServer, ItemsCollection itemsCollection, IElementFactory elementFactory, ChatBox chatBox) : base(mtaServer)
    {
        itemsCollection.Add(1, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 4,
            Name = "Test item id 1",
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(2, new ItemsCollectionItem
        {
            Size = 2,
            StackSize = 4,
            Name = "Foo item id 2",
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(3, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 1,
            Name = "Sample weapon",
            AvailableActions = ItemAction.None,
        });
        itemsCollection.Add(4, new ItemsCollectionItem
        {
            Size = 0.05m,
            StackSize = 10,
            Name = "item size 0.05",
            AvailableActions = ItemAction.None,
        });

        _chatBox = chatBox;
    }

    protected override void PlayerJoined(RealmPlayer player)
    {
        player.Inventory.PrimarySet += HandlePrimarySet;
    }


    protected override void PlayerLeft(RealmPlayer player)
    {
        player.Inventory.PrimarySet -= HandlePrimarySet;

        var primary = player.Inventory.Primary;
        if (primary != null)
        {
            primary.ItemAdded -= HandleInventoryItemAdded;
            primary.ItemRemoved -= HandleInventoryItemRemoved;
        }
    }

    private void HandlePrimarySet(IElementInventoryFeature inventoryService, Inventory inventory)
    {
        inventory.ItemAdded += HandleInventoryItemAdded;
        inventory.ItemRemoved += HandleInventoryItemRemoved;
    }

    private void HandleInventoryItemRemoved(Inventory inventory, Item item)
    {
        var itemId = item.ItemId;
        switch (itemId)
        {
            case 3:
                if (!inventory.HasItemById(itemId))
                {
                    if(inventory.Owner is RealmPlayer player)
                    {
                        player.Weapons.Add(new SlipeServer.Server.Elements.Structs.Weapon(WeaponId.Bat, 1));
                        _chatBox.OutputTo(player, "Bat taken");
                    }
                }
                break;
        }
    }

    private void HandleInventoryItemAdded(Inventory inventory, Item item)
    {
        var itemId = item.ItemId;
        switch (itemId)
        {
            case 3:
                if (inventory.Owner is RealmPlayer player)
                {
                    if (!player.Weapons.Any(x => x.Type == WeaponId.Bat && x.Ammo > 0))
                    {
                        player.Weapons.Add(new SlipeServer.Server.Elements.Structs.Weapon(WeaponId.Bat, 1));
                        _chatBox.OutputTo(player, "Bat taken");
                    }
                }
                break;
        }
    }

    private Task Use(Inventory inventory, Item item, ItemAction action)
    {
        switch (action)
        {
            case ItemAction.Use:
                if (inventory.Owner is RealmPlayer player)
                {
                    _chatBox.OutputTo(player, $"Item used: {item.Name}");
                    inventory.RemoveItem(item);
                }
                break;
        }
        return Task.CompletedTask;
    }
}
