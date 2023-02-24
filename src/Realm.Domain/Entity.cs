using System.Diagnostics.CodeAnalysis;

namespace Realm.Domain;

public class Entity : IDisposable
{
    public enum EntityTag
    {
        Unknown,
        Player,
        Ped,
        Vehicle,
        Blip,
        Pickup,
        Marker,
        CollisionShape,
        WorldObject,
    }

    private bool _disposed = false;
    private readonly CancellationTokenSource _cancellationTokenSource;
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

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

    public event Action<Entity>? Destroyed;

    public Entity(IServiceProvider serviceProvider, string name = "", EntityTag tag = EntityTag.Unknown)
    {
        _cancellationTokenSource = new CancellationTokenSource();
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
                property.SetValue(obj, _serviceProvider.GetRequiredService(property.PropertyType));
            }
        }

        if (type != typeof(object))
            InternalInjectProperties(type.BaseType, obj);
    }

    private void InjectProperties<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();

        component.Entity = this;

        InternalInjectProperties(typeof(TComponent), component);
    }

    private void CheckCanBeAdded<TComponent>() where TComponent : Component
    {
        var componentUsageAttribute = typeof(TComponent).GetCustomAttribute<ComponentUsageAttribute>();
        if (componentUsageAttribute == null || componentUsageAttribute.AllowMultiple)
            return;

        if (_components.OfType<TComponent>().Any())
            throw new ComponentCanNotBeAddedException<TComponent>();
    }

    private void InternalAddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        _componentsLock.EnterWriteLock();
        try
        {
            CheckCanBeAdded<TComponent>();
            _components.Add(component);
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

    public TComponent AddComponent<TComponent>() where TComponent : Component, new()
    {
        return AddComponent(new TComponent());
    }

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : Component
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
        }
        catch(Exception)
        {
            DestroyComponent(component);
            throw;
        }

        ComponentAdded?.Invoke(component);

        Task.Run(async () =>
        {
            ThrowIfDisposed();
            try
            {
                await component.InternalLoadAsync();
            }
            catch (Exception)
            {
                DestroyComponent(component);
            }
        });
        return component;
    }
    
    public Task<TComponent> AddComponentAsync<TComponent>() where TComponent : Component, new()
    {
        return AddComponentAsync(new TComponent());
    }

    public async Task<TComponent> AddComponentAsync<TComponent>(TComponent component) where TComponent : Component
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
        catch(Exception)
        {
            DestroyComponent(component);
            throw;
        }
        ComponentAdded?.Invoke(component);
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
        ComponentDetached?.Invoke(component);
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

    private void Destroy()
    {
        if (Destroyed != null)
            Destroyed.Invoke(this);

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
        _cancellationTokenSource.Cancel();
        Destroy();
        _disposed = true;
    }
}
