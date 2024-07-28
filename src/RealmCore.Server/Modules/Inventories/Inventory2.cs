namespace RealmCore.Server.Modules.Inventories;

internal interface IInventoryEvent;
internal record struct InventoryAddItemEvent(InventoryItem inventoryItem) : IInventoryEvent;
internal record struct InventoryRemoveItemEvent(InventoryItem inventoryItem) : IInventoryEvent;
internal record struct InventoryChangedItemEvent(InventoryItem inventoryItem) : IInventoryEvent;

public struct InventoryAccess : IDisposable
{
    private readonly Inventory2 _inventory;
    private readonly ItemsCollection _itemsCollection;
    private readonly Queue<IInventoryEvent> _events = [];
    private readonly HashSet<string> _changedItems = [];
    private List<InventoryItem> _items;

    public IReadOnlyList<InventoryItem> Items => _items;

    internal InventoryAccess(Inventory2 inventory, ItemsCollection itemsCollection)
    {
        _inventory = inventory;
        _itemsCollection = itemsCollection;
        _items = [.. inventory.Items];
    }

    public bool AddItem(uint itemId, uint number = 1, ItemMetadata? metaData = null)
    {
        var inventoryItem = new InventoryItem(_itemsCollection, itemId, number, metaData);
        if (HasSpaceFor(inventoryItem))
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

    private bool HasSpaceFor(InventoryItem inventoryItem)
    {
        var number = GetNumber();
        return number + (inventoryItem.Size * number) > _inventory.Size;
    }

    public void Dispose()
    {
        _inventory.Close(_events);
    }
}

public class Inventory2
{
    private readonly List<InventoryItem> _items = [];
    private readonly SemaphoreSlim _semaphore = new(1,1);
    public event Action<Inventory2, InventoryItem>? ItemAdded;
    public event Action<Inventory2, InventoryItem>? ItemRemoved;
    public event Action<Inventory2, InventoryItem>? ItemChanged;
    public event Action<Inventory2, decimal>? SizeChanged;

    private decimal _size;
    private readonly ItemsCollection _itemsCollection;

    internal List<InventoryItem> Items => _items;

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


    public Inventory2(decimal size, ItemsCollection itemsCollection)
    {
        _size = size;
        _itemsCollection = itemsCollection;
    }

    private void AddItem(InventoryItem item)
    {
        _items.Add(item);
    }

    private bool RemoveItem(InventoryItem item)
    {
        return _items.Remove(item);
    }

    public InventoryItem[] GetItems()
    {
        _semaphore.Wait();
        try
        {
            return [.. _items];
        }
        finally
        {
            _semaphore.Release();
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
                        foreach (Action<Inventory2, InventoryItem> callback in ItemAdded.GetInvocationList())
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
                        foreach (Action<Inventory2, InventoryItem> callback in ItemChanged.GetInvocationList())
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
                        foreach (Action<Inventory2, InventoryItem> callback in ItemRemoved.GetInvocationList())
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
