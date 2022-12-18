using Realm.Domain.Inventory;
using Realm.Module.Scripting.Extensions;

namespace Realm.Domain.Components.Common;

[NoDefaultScriptAccess]
public class InventoryComponent : Component
{
    private Element _owner = default!;
    private readonly List<Item> _items = new();
    public event Action<InventoryComponent>? NotifyNotSavedState;

    [ScriptMember("capacity")]
    public int Capacity { get; set; }
    [ScriptMember("usedCapacity")]
    public int UsedCapacity => _items.Sum(x => x.Size);

    [ScriptMember("items")]
    public object Items => _items.ToArray().ToScriptArray();

    public InventoryComponent()
    {
    }

    [ScriptMember("addItem")]
    public bool AddItem(Item item)
    {
        if (item.Size + UsedCapacity > Capacity)
            return false;
        _items.Add(item);
        NotifyNotSavedState?.Invoke(this);
        return true;
    }

    [ScriptMember("toString")]
    public override string ToString() => "Inventory";
}
