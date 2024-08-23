namespace RealmCore.Server.Modules.Inventories;

internal interface IInventoryEvent;
internal record struct InventoryAddItemEvent(int version, InventoryItem inventoryItem, uint number) : IInventoryEvent;
internal record struct InventoryItemNumberEvent(int version, InventoryItem inventoryItem, uint number, bool add) : IInventoryEvent;
internal record struct InventoryRemoveItemEvent(int version, InventoryItem inventoryItem, uint number) : IInventoryEvent;
internal record struct InventoryChangedItemEvent(int version, InventoryItem inventoryItem) : IInventoryEvent;

public interface IPersistentInventory
{
    public int Id { get; set; }
}

internal sealed class Version
{
    public int Current { get; set; }
}

public readonly struct InventoryAccess : IDisposable
{
    private readonly Inventory _inventory;
    private readonly InventoryItem[] _items;
    private readonly ItemsCollection _itemsCollection;
    private readonly Queue<IInventoryEvent> _events = [];
    private readonly Version _version = new();
    private readonly Dictionary<InventoryItem, uint> _removedItems = [];
    private readonly HashSet<string> _changedItems = [];
    private readonly List<InventoryItem> _newItems = [];

    public IEnumerable<InventoryItem> Items
    {
        get
        {
            foreach (var item in _items.Concat(_newItems))
            {
                if (_removedItems.TryGetValue(item, out var removedNumber))
                {
                    if (item.Number <= removedNumber)
                        continue;
                }
                yield return item;
            }
        }
    }

    public decimal Number => Items.Sum(x => x.Size * x.Number);

    internal InventoryAccess(Inventory inventory, ItemsCollection itemsCollection)
    {
        _inventory = inventory;
        _itemsCollection = itemsCollection;
        _items = [.. inventory.Items];
    }

    public bool TryAddItem(uint itemId, uint number = 1, ItemMetadata? metaData = null, bool force = false, bool tryStack = true)
    {
        if (number == 0)
            return false;

        if (!force && !HasSpaceFor(itemId, number))
            return false;

        var itemsCollectionItem = _itemsCollection.Get(itemId);

        if (tryStack)
        {
            var existingItems = Items.Where(x => x.ItemId == itemId).ToArray();
            foreach (var item in existingItems)
            {
                var remainingSpace = itemsCollectionItem.StackSize - item.Number;
                if(remainingSpace > 0)
                {
                    var add = Math.Min(number, remainingSpace);
                    number -= add;

                    item.Number += add;
                    _events.Enqueue(new InventoryItemNumberEvent(_version.Current, item, add, _newItems.Contains(item)));
                }
            }

            while(number > 0)
            {
                var add = Math.Min(number, itemsCollectionItem.StackSize);
                var item = new InventoryItem(_itemsCollection, itemId, add, metaData != null ? new(metaData) : null);
                _newItems.Add(item);
                _events.Enqueue(new InventoryAddItemEvent(_version.Current, item, add));
                number -= add;
            }
            return true;
        }
        else
        {
            if (itemsCollectionItem.StackSize < number)
                return false;

            var item = new InventoryItem(_itemsCollection, itemId, number, metaData != null ? new(metaData) : null);
            _newItems.Add(item);
            _events.Enqueue(new InventoryAddItemEvent(_version.Current, item, number));
            return true;
        }
    }

    public InventoryItem? FindItem(Func<InventoryItem, bool> predicate)
    {
        return _items.Where(predicate).FirstOrDefault();
    }
    
    public bool RemoveItem(InventoryItem inventoryItem, uint number = 1)
    {
        if (Items.Contains(inventoryItem))
        {
            if (_removedItems.TryGetValue(inventoryItem, out var value))
            {
                if (value + number > inventoryItem.Number)
                    return false;

                _removedItems[inventoryItem] += number;
            }
            else
            {
                if (number > inventoryItem.Number)
                    return false;

                _removedItems[inventoryItem] = number;
            }
            _events.Enqueue(new InventoryRemoveItemEvent(_version.Current, inventoryItem, number));
            return true;
        }
        return false;
    }

    private decimal GetNumber() => _items.Sum(x => x.Size * x.Number);

    public bool HasSpaceFor(uint itemId, uint number = 1)
    {
        var inventoryItem = _itemsCollection.Get(itemId);
        return Number + (inventoryItem.Size * number) <= _inventory.Size;
    }

    public bool HasSpaceFor(InventoryItem inventoryItem)
    {
        var number = GetNumber();
        return number + (inventoryItem.Size * number) > _inventory.Size;
    }

    public bool TransferItem(InventoryAccess destination, string localId, uint number)
    {
        if (number == 0)
            return false;

        var item = _items.Where(x => x.LocalId == localId).FirstOrDefault();
        if (item == null)
            return false;

        if (!destination.HasSpaceFor(item.ItemId, number))
            return false;

        if (!RemoveItem(item, number))
            return false;

        return destination.TryAddItem(item.ItemId, number, item.MetaData);
    }

    public void Clear()
    {
        InventoryItem[] itemsToRemove = [.. _items];
        foreach (var inventoryItem in itemsToRemove)
        {
            RemoveItem(inventoryItem);
        }
    }

    public void Dispose()
    {
        _inventory.Close(_events);
    }
}

public class Inventory
{
    private readonly List<InventoryItem> _items = [];
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    public event Action<Inventory, InventoryItem>? ItemAdded;
    public event Action<Inventory, InventoryItem>? ItemRemoved;
    public event Action<Inventory, InventoryItem>? ItemChanged;
    public event Action<Inventory, decimal>? SizeChanged;

    private decimal _size;
    private readonly ItemsCollection _itemsCollection;

    internal InventoryItem[] Items => [.. _items];

    public decimal Size
    {
        get
        {
            return _size;
        }
        set
        {
            SizeChanged?.Invoke(this, value);
            _size = value;
        }
    }

    public bool IsFull => Number >= Size;

    public decimal Number
    {
        get
        {
            _semaphore.Wait();
            try
            {
                return GetNumber();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public Inventory(decimal size, ItemsCollection itemsCollection, IEnumerable<InventoryItem>? items = null)
    {
        _size = size;
        _itemsCollection = itemsCollection;

        if (items != null)
            _items = [.. items];
    }

    private void AddItem(InventoryItem item, uint number)
    {
        var existingItem = _items.FirstOrDefault(x => x == item);
        if(existingItem != null)
        {
            existingItem.Number += number;
        }
        else
        {
            item.Number = number;
            _items.Add(item);
        }
    }

    private bool RemoveItem(InventoryItem item, uint number = 1)
    {
        if (item.Number < number)
            return false;

        if (item.Number > number)
        {
            item.Number -= (uint)number;
            return true;
        }
        else
        {
            return _items.Remove(item);
        }
    }

    public InventoryAccess Open(TimeSpan? timeout = null)
    {
        if (!_semaphore.Wait(timeout ?? TimeSpan.FromSeconds(1)))
            throw new TimeoutException();

        return new InventoryAccess(this, _itemsCollection);
    }

    internal decimal GetNumber() => _items.Sum(x => x.Size * x.Number);

    internal bool HasSpaceFor(InventoryItem inventoryItem)
    {
        var number = GetNumber();
        return number + (inventoryItem.Size * number) > Size;
    }

    internal void Close(Queue<IInventoryEvent> events)
    {
        IInventoryEvent[] inventoryEvents = [.. events];
        foreach (var inventoryEvent in inventoryEvents)
        {
            switch (inventoryEvent)
            {
                case InventoryAddItemEvent addItemEvent:
                    AddItem(addItemEvent.inventoryItem, addItemEvent.number);
                    break;
                case InventoryItemNumberEvent addItemEvent when addItemEvent.add:
                    AddItem(addItemEvent.inventoryItem, addItemEvent.number);
                    break;
                case InventoryRemoveItemEvent removeItemEvent:
                    RemoveItem(removeItemEvent.inventoryItem, removeItemEvent.number);
                    break;
            }
        }

        _semaphore.Release();

        List<Exception> exceptions = [];
        while (events.TryDequeue(out IInventoryEvent? e))
        {
            switch (e)
            {
                case InventoryAddItemEvent addItemEvent:
                    if (ItemAdded != null)
                    {
                        foreach (Action<Inventory, InventoryItem> callback in ItemAdded.GetInvocationList())
                        {
                            try
                            {
                                ItemAdded?.Invoke(this, addItemEvent.inventoryItem);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }
                    break;
                case InventoryItemNumberEvent itemNumberEvent:
                    if (ItemChanged != null)
                    {
                        foreach (Action<Inventory, InventoryItem> callback in ItemChanged.GetInvocationList())
                        {
                            try
                            {
                                ItemChanged?.Invoke(this, itemNumberEvent.inventoryItem);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }
                    break;
                case InventoryChangedItemEvent changeItemEvent:
                    if (ItemChanged != null)
                    {
                        foreach (Action<Inventory, InventoryItem> callback in ItemChanged.GetInvocationList())
                        {
                            try
                            {
                                ItemChanged?.Invoke(this, changeItemEvent.inventoryItem);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }
                    break;
                case InventoryRemoveItemEvent removeItemEvent:
                    if (ItemRemoved != null)
                    {
                        foreach (Action<Inventory, InventoryItem> callback in ItemRemoved.GetInvocationList())
                        {
                            try
                            {
                                ItemRemoved?.Invoke(this, removeItemEvent.inventoryItem);
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex);
                            }
                        }
                    }
                    break;
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException(exceptions);
        }
    }
}

public class ElementInventory : Inventory
{
    public Element Owner { get; }

    public ElementInventory(Element owner, decimal size, ItemsCollection itemsCollection, IEnumerable<InventoryItem>? items = null) : base(size, itemsCollection, items)
    {
        Owner = owner;
    }
}

public class PersistentElementInventory : ElementInventory, IPersistentInventory
{
    public int Id { get; set; }

    public PersistentElementInventory(int id, Element owner, decimal size, ItemsCollection itemsCollection, IEnumerable<InventoryItem>? items = null) : base(owner, size, itemsCollection, items)
    {
        Id = id;
    }

    public static PersistentElementInventory CreateFromData(Element element, InventoryData inventory, ItemsCollection itemsCollection)
    {
        var items = inventory.InventoryItems
        .Select(x =>
                new InventoryItem(itemsCollection, x.ItemId, x.Number, JsonConvert.DeserializeObject<ItemMetadata>(x.MetaData, _jsonSerializerSettings), x.Id)
            )
            .ToList();
        return new PersistentElementInventory(inventory.Id, element, inventory.Size, itemsCollection, items);
    }

    private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Converters = new List<JsonConverter> { DoubleConverter.Instance }
    };

    public static InventoryData CreateData(IPersistentInventory persistentInventory)
    {
        var inventory = (Inventory)persistentInventory;
        var data = new InventoryData
        {
            Size = inventory.Size,
            Id = persistentInventory.Id,
            InventoryItems = MapItems(inventory.Items),
        };
        return data;
    }

    private static List<InventoryItemData> MapItems(IReadOnlyList<InventoryItem> items)
    {
        var itemsData = items.Select(item => InventoryItem.CreateData(item)).ToList();

        return itemsData;
    }
}
