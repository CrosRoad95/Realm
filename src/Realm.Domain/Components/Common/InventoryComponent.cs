using Realm.Domain.Inventory;

namespace Realm.Domain.Components.Common;

public class InventoryComponent : Component
{
    private readonly List<Item> _items = new();
    public event Action<InventoryComponent>? NotifyNotSavedState;

    public int Capacity { get; set; }
    public int UsedCapacity => _items.Sum(x => x.Size);

    public IEnumerable<Item> Items => _items;

    public InventoryComponent()
    {
    }

    public bool AddItem(Item item)
    {
        if (item.Size + UsedCapacity > Capacity)
            return false;
        _items.Add(item);
        NotifyNotSavedState?.Invoke(this);
        return true;
    }
}
