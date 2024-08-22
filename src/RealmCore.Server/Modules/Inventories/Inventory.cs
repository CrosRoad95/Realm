namespace RealmCore.Server.Modules.Inventories;

internal interface IInventoryEvent;
internal record struct InventoryAddItemEvent(InventoryItem inventoryItem) : IInventoryEvent;
internal record struct InventoryRemoveItemEvent(InventoryItem inventoryItem) : IInventoryEvent;
internal record struct InventoryChangedItemEvent(InventoryItem inventoryItem) : IInventoryEvent;

public interface IPersistentInventory
{
    public int Id { get; set; }
}
public readonly struct InventoryAccess : IDisposable
{
    private readonly Inventory _inventory;
    private readonly ItemsCollection _itemsCollection;
    private readonly Queue<IInventoryEvent> _events = [];
    private readonly HashSet<string> _changedItems = [];
    private readonly List<InventoryItem> _items;

    public IReadOnlyList<InventoryItem> Items => _items;

    internal InventoryAccess(Inventory inventory, ItemsCollection itemsCollection)
    {
        _inventory = inventory;
        _itemsCollection = itemsCollection;
        _items = [.. inventory.Items];
    }

    public bool TryAddItem(uint itemId, uint number = 1, ItemMetadata? metaData = null, bool force = false)
    {
        var inventoryItem = new InventoryItem(_itemsCollection, itemId, number, metaData);
        if (force || HasSpaceFor(inventoryItem))
        {
            return false;
        }

        _items.Add(inventoryItem);
        _events.Enqueue(new InventoryAddItemEvent(inventoryItem));
        return true;
    }
    

    public InventoryItem? FindItem(Func<InventoryItem, bool> predicate)
    {
        return _items.Where(predicate).FirstOrDefault();
    }

    public bool RemoveItem(InventoryItem inventoryItem)
    {
        if (_items.Remove(inventoryItem))
        {
            _events.Enqueue(new InventoryRemoveItemEvent(inventoryItem));
            return true;
        }
        return false;
    }

    private decimal GetNumber() => _items.Sum(x => x.Size * x.Number);

    public bool HasSpaceFor(uint itemId, int number = 1)
    {
        var inventoryItem = _itemsCollection.Get(itemId);
        return number + (inventoryItem.Size * number) > _inventory.Size;
    }
    
    private bool HasSpaceFor(InventoryItem inventoryItem)
    {
        var number = GetNumber();
        return number + (inventoryItem.Size * number) > _inventory.Size;
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
    private readonly SemaphoreSlim _semaphore = new(1,1);
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

        if(items != null)
            _items = [.. items];
    }

    private void AddItem(InventoryItem item)
    {
        _items.Add(item);
    }

    private bool RemoveItem(InventoryItem item)
    {
        return _items.Remove(item);
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
                    AddItem(addItemEvent.inventoryItem);
                    break;
                case InventoryRemoveItemEvent removeItemEvent:
                    RemoveItem(removeItemEvent.inventoryItem);
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
        
        if(exceptions.Count > 0)
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
