using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Server;

public class Entity : IDisposable
{
    private object _transactionLock = new();
    private byte _version;
    private bool _hasPendingTransaction;

    private bool _disposed = false;

    public string Id { get; } = Guid.NewGuid().ToString();
    public EntityTag Tag { get; }
    public string Name { get; set; } = "";

    private readonly ReaderWriterLockSlim _componentsLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly List<Component> _components = new();
    public IEnumerable<Component> Components
    {
        get
        {
            _componentsLock.EnterReadLock();
            var components = new List<Component>(_components);
            _componentsLock.ExitReadLock();
            return components.AsReadOnly();
        }
    }
    
    public int ComponentsCount
    {
        get
        {
            _componentsLock.EnterReadLock();
            var count = _components.Count;
            _componentsLock.ExitReadLock();
            return count;
        }
    }

    private readonly IServiceProvider _serviceProvider;

    public Transform Transform { get; private set; }

    public event Action<Component>? ComponentAdded;
    public event Action<Component>? ComponentDetached;

    public event Action<Entity>? Disposed;

    internal Player Player => GetRequiredComponent<PlayerElementComponent>().Player;
    internal Vehicle Vehicle => GetRequiredComponent<VehicleElementComponent>().Vehicle;
    internal Element Element => GetRequiredComponent<ElementComponent>().Element;
    
    public Entity(IServiceProvider serviceProvider, string name = "", EntityTag tag = EntityTag.Unknown)
    {
        _serviceProvider = serviceProvider;
        Name = name;
        Tag = tag;
        Transform = new Transform(this);
    }

    internal bool TryGetElement(out Element element)
    {
        if(TryGetComponent(out ElementComponent elementComponent))
        {
            element = elementComponent.Element;
            return true;
        }

        element = null!;
        return false;
    }

    private void InternalInjectProperties(Type type, object obj)
    {
        foreach (var property in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
        {
            if (property.GetCustomAttribute<InjectAttribute>() != null)
            {
                var service = _serviceProvider.GetService(property.PropertyType);
                if (service == null)
                    throw new Exception($"Could not inject service of type {property.PropertyType}");
                property.SetValue(obj, _serviceProvider.GetRequiredService(property.PropertyType));
            }
        }

        if (type != typeof(object))
            InternalInjectProperties(type.BaseType, obj);
    }

    internal void InjectProperties<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();

        InternalInjectProperties(typeof(TComponent), component);
    }

    protected void CheckCanBeAdded<TComponent>() where TComponent : Component
    {
        var componentUsageAttribute = typeof(TComponent).GetCustomAttribute<ComponentUsageAttribute>();
        if (componentUsageAttribute == null || componentUsageAttribute.AllowMultiple)
            return;

        if (_components.OfType<TComponent>().Any())
            throw new ComponentCanNotBeAddedException<TComponent>();
    }

    protected void InternalAddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        _componentsLock.EnterWriteLock();
        try
        {
            CheckCanBeAdded<TComponent>();
            _components.Add(component);
            lock (_transactionLock)
                component._version = _version;
        }
        finally
        {
            _componentsLock.ExitWriteLock();
        }
    }

    protected void OnComponentAdded(Component component)
    {
        ComponentAdded?.Invoke(component);
    }

    public TComponent AddComponent<TComponent>() where TComponent : Component, new()
    {
        return AddComponent(new TComponent());
    }

    public TComponent AddComponentFromDI<TComponent>() where TComponent : Component
    {
        TComponent component = _serviceProvider.GetRequiredService<TComponent>();
        return AddComponent(component);
    }

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        if (component is AsyncComponent)
            throw new ArgumentException("Can not add async component to non async entity");

        ThrowIfDisposed();

        if (!component.TrySetEntity(this))
            throw new Exception("Component already attached to other entity");

        InjectProperties(component);
        InternalAddComponent(component);
        try
        {
            component.InternalLoad();
        }
        catch (Exception)
        {
            component.TryRemoveEntity(() =>
            {
                DestroyComponent(component);
            });
            throw;
        }

        OnComponentAdded(component);
        return component;
    }


    public Task<TComponent> AddComponentAsync<TComponent>() where TComponent : AsyncComponent, new()
    {
        return AddComponentAsync(new TComponent());
    }

    public async Task<TComponent> AddComponentAsync<TComponent>(TComponent component) where TComponent : AsyncComponent
    {
        ThrowIfDisposed();

        if (!component.TrySetEntity(this))
            throw new Exception("Component already attached to other entity");

        InjectProperties(component);
        InternalAddComponent(component);

        try
        {
            component.InternalLoad();
            await component.InternalLoadAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            DestroyComponent(component);
            throw;
        }
        OnComponentAdded(component);
        return component;
    }

    private TComponent? InternalGetComponent<TComponent>() where TComponent : Component
    {
        return _components.OfType<TComponent>().FirstOrDefault();
    }

    private IEnumerable<TComponent> InternalGetComponents<TComponent>() where TComponent : Component
    {
        return _components.OfType<TComponent>();
    }
    
    public TComponent? GetComponent<TComponent>() where TComponent : Component
    {
        ThrowIfDisposed();

        _componentsLock.EnterReadLock();
        try
        {
            return InternalGetComponent<TComponent>();
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
    }
    
    public IReadOnlyList<TComponent> GetComponents<TComponent>() where TComponent : Component
    {
        ThrowIfDisposed();

        _componentsLock.EnterReadLock();
        try
        {
            return InternalGetComponents<TComponent>().ToList();
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
    }

    internal bool InternalHasComponent<TComponent>(TComponent component) where TComponent : Component
    {
        return _components.Contains(component);
    }

    internal bool InternalHasComponent<TComponent>() where TComponent : Component
    {
        return _components.OfType<TComponent>().Any();
    }

    public bool HasComponent<TComponent>() where TComponent : Component
    {
        ThrowIfDisposed();

        _componentsLock.EnterReadLock();
        try
        {
            return InternalHasComponent<TComponent>();
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
    }

    public bool HasComponent<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();
        bool has;

        _componentsLock.EnterReadLock();
        try
        {
            has = _components.Contains(component);
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
        return has;
    }

    public bool HasComponent(Type type)
    {
        ThrowIfDisposed();
        bool has;

        _componentsLock.EnterReadLock();
        try
        {
            has = _components.Where(x => x.GetType() == type).Any();
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
        return has;
    }

    public bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent component) where TComponent : Component
    {
        component = GetComponent<TComponent>();
        return component != null;
    }

    public TComponent GetRequiredComponent<TComponent>() where TComponent : Component
    {
        var component = InternalGetComponent<TComponent>();
        return component == null ? throw new ComponentNotFoundException<TComponent>() : component;
    }

    private void InternalDetachComponent<TComponent>(TComponent component) where TComponent : Component
    {
        component.TryRemoveEntity(() =>
        {
            ComponentDetached?.Invoke(component);
            _components.Remove(component);
        });
    }
    
    public void DetachComponent<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();

        if (component.Entity == this)
        {
            _componentsLock.EnterWriteLock();
            try
            {
                InternalDetachComponent(component);
            }
            finally
            {
                _componentsLock.ExitWriteLock();
            }
        }
    }

    public void DestroyComponent<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();
        component.Dispose();
        DetachComponent(component);
    }

    public bool TryDestroyComponent<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();

        _componentsLock.EnterWriteLock();
        try
        {
            if(InternalHasComponent(component))
            {
                component.Dispose();
                InternalDetachComponent(component);
                return true;
            }
        }
        catch (ObjectDisposedException)
        { }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            _componentsLock.ExitWriteLock();
        }
        return false;
    }

    public bool TryDestroyComponent<TComponent>() where TComponent : Component
    {
        ThrowIfDisposed();

        _componentsLock.EnterWriteLock();
        try
        {
            var component = InternalGetComponent<TComponent>();
            if (component == null)
                return false;
            try
            {
                component.Dispose();
                InternalDetachComponent(component);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception)
            {
                throw;
            }
        }
        finally
        {
            _componentsLock.ExitWriteLock();
        }
        return true;
    }

    public void DestroyComponent<TComponent>() where TComponent : Component
    {
        ThrowIfDisposed();
        var component = GetRequiredComponent<TComponent>();
        component.Dispose();
        DetachComponent(component);
    }

    public override string ToString() => Name;

    protected void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Entity));
    }

    public virtual void Dispose()
    {
        ThrowIfDisposed();

        Disposed?.Invoke(this);

        List<Component> componentsToDestroy;
        _componentsLock.EnterReadLock();
        try
        {
            componentsToDestroy = _components.AsEnumerable().Reverse().ToList();
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }

        foreach (var component in componentsToDestroy)
            DestroyComponent(component);

        _disposed = true;
    }

    public IEntityComponentsTransaction BeginComponentTransaction()
    {
        ThrowIfDisposed();

        lock (_transactionLock)
        {
            if (_hasPendingTransaction)
                throw new InvalidOperationException("Transaction for this entity is already in progress");
            _hasPendingTransaction = true;
            _version++;
            return new EntityComponentsTransaction(_version, this);
        }
    }

    public int Commit(IEntityComponentsTransaction transaction)
    {
        ThrowIfDisposed();

        if (transaction.Entity != this)
            throw new InvalidOperationException("Transaction does not belong to this entity");

        if (!transaction.TryClose())
            throw new InvalidOperationException("Transaction already committed");

        int committedComponents = 0;
        _componentsLock.EnterWriteLock();
        try
        {
            committedComponents = _components.Count(x => x._version == transaction.Version);
        }
        finally
        {
            _componentsLock.ExitWriteLock();
            lock (_transactionLock)
            {
                _hasPendingTransaction = false;
                _version++;
            }
        }

        return committedComponents;
    }

    public int Rollback(IEntityComponentsTransaction transaction)
    {
        ThrowIfDisposed();

        if (transaction.Entity != this)
            throw new InvalidOperationException("Transaction does not belong to this entity");

        if (!transaction.TryClose())
            throw new InvalidOperationException("Transaction already committed");

        int rollbackedComponents = 0;
        var components = Components;
        try
        {
            foreach (var item in components)
            {
                if (item._version == transaction.Version)
                {
                    DestroyComponent(item);
                    rollbackedComponents++;
                }
            }
        }
        finally
        {
            lock (_transactionLock)
            {
                _hasPendingTransaction = false;
                _version++;
            }
        }

        return rollbackedComponents;
    }
}
