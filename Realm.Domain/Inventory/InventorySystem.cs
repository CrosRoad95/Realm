﻿using Realm.Scripting.Extensions;

namespace Realm.Domain.Inventory;

[Serializable]
[NoDefaultScriptAccess]
public class InventorySystem : ISerializable
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


    public InventorySystem(SerializationInfo info, StreamingContext context)
    {
        _items = (List<PlayerItem>?)info.GetValue("Items", typeof(List<PlayerItem>)) ?? throw new SerializationException();
    }

    public void SetOwner(Element element)
    {
        if (_owner != null)
            throw new Exception("Inventory system already have an owner.");
        _owner = element;
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Items", _items);
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

    public static InventorySystem CreateFromString(string str)
    {
        return JsonConvert.DeserializeObject<InventorySystem>(str, new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
        }) ?? throw new JsonSerializationException("Failed to deserialize InventorySystem");
    }
}