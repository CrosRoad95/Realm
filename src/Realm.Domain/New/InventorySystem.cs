using Realm.Domain.Inventory;
using Realm.Module.Scripting.Extensions;

namespace Realm.Domain.New;

[NoDefaultScriptAccess]
public class InventorySystem : Component
{
    private Element _owner = default!;
    private readonly List<PlayerItem> _items = new();
    public event Action<InventorySystem>? NotifyNotSavedState;

    [ScriptMember("capacity")]
    public int Capacity { get; set; }
    [ScriptMember("usedCapacity")]
    public int UsedCapacity => _items.Sum(x => x.Size);

    [ScriptMember("items")]
    public object Items => _items.ToArray().ToScriptArray();

    public InventorySystem()
    {
    }

    [ScriptMember("addItem")]
    public bool AddItem(PlayerItem item)
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
