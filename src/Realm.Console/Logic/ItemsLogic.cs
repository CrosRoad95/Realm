using Realm.Domain.Inventory;
using static Realm.Domain.Registries.ItemRegistryEntry;

namespace Realm.Console.Logic;

public class ItemsLogic
{
    public ItemsLogic(ItemsRegistry itemsRegistry)
    {
        itemsRegistry.UseCallback = Use;
        itemsRegistry.AddItem(1, new ItemRegistryEntry
        {
            Size= 1,
            StackSize = 10,
            Name = "Test item id 1",
            AvailiableActions = ItemAction.DefaultUse,
        });
    }

    private Task Use(InventoryComponent inventoryComponent, Item item, ItemAction action)
    {
        switch(action)
        {
            case ItemAction.DefaultUse:
                inventoryComponent.RemoveItem(item);
                break;
        }
        return Task.CompletedTask;
    }
}
