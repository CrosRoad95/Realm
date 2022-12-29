using Realm.Domain.Registries;

namespace Realm.Console.Logic;

public class ItemsLogic
{
    public ItemsLogic(ItemsRegistry itemsRegistry)
    {
        itemsRegistry.AddItem(1, new ItemRegistryEntry
        {
            Size= 1,
            Name = "Test item id 1",
        });
    }
}
