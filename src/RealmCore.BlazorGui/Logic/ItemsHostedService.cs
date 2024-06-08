namespace RealmCore.BlazorGui.Logic;

public class ItemsHostedService : PlayerLifecycle, IHostedService
{
    private readonly ChatBox _chatBox;

    public ItemsHostedService(PlayersEventManager playersEventManager, ItemsCollection itemsCollection, ChatBox chatBox) : base(playersEventManager)
    {
        itemsCollection.Add(1, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 4,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(2, new ItemsCollectionItem
        {
            Size = 2,
            StackSize = 4,
            AvailableActions = ItemAction.Use,
        });
        itemsCollection.Add(3, new ItemsCollectionItem
        {
            Size = 1,
            StackSize = 1,
            AvailableActions = ItemAction.None,
        });
        itemsCollection.Add(4, new ItemsCollectionItem
        {
            Size = 0.05m,
            StackSize = 10,
            AvailableActions = ItemAction.None,
        });

        _chatBox = chatBox;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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

    private void HandleInventoryItemRemoved(Inventory inventory, InventoryItem item)
    {
        var itemId = item.ItemId;
        switch (itemId)
        {
            case 3:
                if (!inventory.HasItemById(itemId))
                {
                    if (inventory.Owner is RealmPlayer player)
                    {
                        player.Weapons.Add(new SlipeServer.Server.Elements.Structs.Weapon(WeaponId.Bat, 1));
                        _chatBox.OutputTo(player, "Bat taken");
                    }
                }
                break;
        }
    }

    private void HandleInventoryItemAdded(Inventory inventory, InventoryItem item)
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

    private Task Use(Inventory inventory, InventoryItem item, ItemAction action)
    {
        switch (action)
        {
            case ItemAction.Use:
                if (inventory.Owner is RealmPlayer player)
                {
                    _chatBox.OutputTo(player, $"Item used: {item.ItemId}");
                    inventory.RemoveItem(item);
                }
                break;
        }
        return Task.CompletedTask;
    }
}
