using System.Diagnostics.CodeAnalysis;
using Realm.Domain.Components;

namespace Realm.Domain;

public class Entity : IDisposable
{
    private object _transactionLock = new();
    private byte _version;
    private bool _hasPendingTransaction;

    private bool _disposed = false;

    public string Id { get; } = Guid.NewGuid().ToString();
    public EntityTag Tag { get; }
    public string Name { get; set; } = "";

    private readonly ReaderWriterLockSlim _componentsLock = new();
    private readonly List<Component> _components = new();
    public IReadOnlyCollection<Component> Components => _components;

    private readonly IServiceProvider _serviceProvider;

    public Transform Transform { get; private set; }

    public event Action<Component>? ComponentAdded;
    public event Action<Component>? ComponentDetached;

    public event Action<Entity>? Disposed;

    internal Player Player => GetRequiredComponent<PlayerElementComponent>().Player;
    internal Element Element => GetRequiredComponent<ElementComponent>().Element;

    public Entity(IServiceProvider serviceProvider, string name = "", EntityTag tag = EntityTag.Unknown)
    {
        _serviceProvider = serviceProvider;
        Name = name;
        Tag = tag;
        Transform = new Transform(this);
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

    protected void InjectProperties<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();

        component.Entity = this;

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
        catch(Exception)
        {
            throw;
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

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        if (component is AsyncComponent)
            throw new ArgumentException("Can not add async component to non async entity");
        ThrowIfDisposed();

        if (component.Entity != null)
            throw new Exception("Component already attached to other entity");

        InjectProperties(component);
        InternalAddComponent(component);
        try
        {
            component.InternalLoad();
        }
        catch(Exception ex)
        {
            DestroyComponent(component);
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
        if (component.Entity != null)
        {
            throw new Exception("Component already attached to other entity");
        }
        InjectProperties(component);
        InternalAddComponent(component);

        try
        {
            component.InternalLoad();
            await component.InternalLoadAsync();
        }
        catch (Exception)
        {
            DestroyComponent(component);
            throw;
        }
        OnComponentAdded(component);
        return component;
    }


    public TComponent? GetComponent<TComponent>() where TComponent : Component
    {
        ThrowIfDisposed();

        TComponent? element;
        _componentsLock.EnterReadLock();
        try
        {
            element = _components.OfType<TComponent>().FirstOrDefault();
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
        return element;
    }
    
    public bool HasComponent<TComponent>() where TComponent : Component
    {
        ThrowIfDisposed();
        bool has;

        _componentsLock.EnterReadLock();
        try
        {
            has = _components.OfType<TComponent>().Any();
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
        return has;
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
    
    public bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent component) where TComponent : Component
    {
        component = GetComponent<TComponent>();
        return component != null;
    }

    public TComponent GetRequiredComponent<TComponent>() where TComponent : Component
    {
        var component = GetComponent<TComponent>();
        if (component == null)
            throw new ComponentNotFoundException<TComponent>();
        return component;
    }

    public void DetachComponent<TComponent>() where TComponent : Component
    {
        ThrowIfDisposed();
        DetachComponent(GetRequiredComponent<TComponent>());
    }

    public void DetachComponent<TComponent>(TComponent component) where TComponent: Component
    {
        ThrowIfDisposed();

        ComponentDetached?.Invoke(component);
        if (component.Entity == this)
        {
            _componentsLock.EnterWriteLock();
            try
            {
                _components.Remove(component);
            }
            finally
            {
                _componentsLock.ExitWriteLock();
            }
            component.Entity = null!;
        }
    }

    public void DestroyComponent<TComponent>(TComponent component) where TComponent: Component
    {
        ThrowIfDisposed();
        component.Dispose();
        DetachComponent(component);
    }
    
    public bool TryDestroyComponent<TComponent>(TComponent component) where TComponent: Component
    {
        ThrowIfDisposed();

        if(HasComponent(component))
        {
            component.Dispose();
            DetachComponent(component);
            return true;
        }

        return false;
    }

    public bool TryDestroyComponent<TComponent>() where TComponent: Component
    {
        ThrowIfDisposed();
        var component = GetComponent<TComponent>();
        if(component != null)
        {
            component.Dispose();
            DetachComponent(component);
            return true;
        }
        return false;
    }

    public void DestroyComponent<TComponent>() where TComponent: Component
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
            throw new ObjectDisposedException(nameof(Objective));
    }

    public virtual void Dispose()
    {
        ThrowIfDisposed();

        if (Disposed != null)
            Disposed.Invoke(this);

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
        
        if(!transaction.TryClose())
            throw new InvalidOperationException("Transaction already commited");

        int commitedComponents = 0;
        _componentsLock.EnterWriteLock();
        try
        {
            commitedComponents = _components.Count(x => x._version == transaction.Version);
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

        return commitedComponents;
    }

    public int Rollback(IEntityComponentsTransaction transaction)
    {
        ThrowIfDisposed();

        if(transaction.Entity != this)
            throw new InvalidOperationException("Transaction does not belong to this entity");

        if (!transaction.TryClose())
            throw new InvalidOperationException("Transaction already commited");

        int rollbackedComponents = 0;
        List<Component> components;
        _componentsLock.EnterReadLock();
        try
        {
            components = Components.ToList();
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }

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

    public TService GetRequiredService<TService>() where TService : class
        => _serviceProvider.GetRequiredService<TService>();
}
