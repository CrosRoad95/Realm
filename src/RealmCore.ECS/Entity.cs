using RealmCore.ECS.Attributes;
using RealmCore.ECS.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace RealmCore.ECS;

public sealed class Entity : IDisposable
{
    private bool _disposed = false;

    public string Id { get; } = Guid.NewGuid().ToString();

    private readonly ReaderWriterLockSlim _componentsLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly List<Component> _components = new();
    public IEnumerable<IComponent> Components
    {
        get
        {
            _componentsLock.EnterReadLock();
            var components = new List<Component>(_components);
            _componentsLock.ExitReadLock();
            return components;
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

    public Transform Transform => GetRequiredComponent<Transform>();

    public event Action<Component>? ComponentAdded;
    public event Action<Component>? ComponentDetached;

    public event Action<Entity>? Disposed;
    public event Action<Entity>? PreDisposed;

    public Entity() { }

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
        if (component is AsyncComponent)
            throw new ArgumentException("Can not add async component using sync method");

        ThrowIfDisposed();

        if (!component.TrySetEntity(this))
            throw new Exception("Component already attached to other entity");

        InternalAddComponent(component);
        try
        {
            component.InternalAttach();
        }
        catch (Exception)
        {
            component.TryRemoveEntity(() =>
            {
                DestroyComponent(component);
            });
            throw;
        }

        ComponentAdded?.Invoke(component);
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

        InternalAddComponent(component);

        try
        {
            component.InternalAttach();
            await component.InternalLoadAsync().ConfigureAwait(false);
        }
        catch (Exception)
        {
            DestroyComponent(component);
            throw;
        }
        ComponentAdded?.Invoke(component);
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
    
    public TComponent? FindComponent<TComponent>(Func<TComponent, bool> callback) where TComponent : Component
    {
        ThrowIfDisposed();

        _componentsLock.EnterReadLock();
        try
        {
            foreach (var component in _components.OfType<TComponent>())
            {
                if (callback(component))
                    return component;
            }
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
        return null;
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
    
    public bool HasComponent<TComponent>(Func<TComponent, bool> callback) where TComponent : Component
    {
        ThrowIfDisposed();

        _componentsLock.EnterReadLock();
        try
        {
            foreach (var component in _components.OfType<TComponent>())
            {
                if (callback(component))
                    return true;
            }
        }
        finally
        {
            _componentsLock.ExitReadLock();
        }
        return false;
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
        component = GetComponent<TComponent>()!;
        return component != null;
    }

    public TComponent GetRequiredComponent<TComponent>() where TComponent : Component
    {
        var component = InternalGetComponent<TComponent>();
        return component ?? throw new ComponentNotFoundException<TComponent>();
    }

    private void InternalDetachComponent<TComponent>(TComponent component) where TComponent : Component
    {
        component.TryRemoveEntity(() =>
        {
            try
            {
                ComponentDetached?.Invoke(component);
                component.InternalDetach();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _components.Remove(component);
            }
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
        DetachComponent(component);
        component.Dispose();
    }

    public bool TryDestroyComponent<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();

        _componentsLock.EnterWriteLock();
        try
        {
            if (InternalHasComponent(component))
            {
                InternalDetachComponent(component);
                component.Dispose();
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
                InternalDetachComponent(component);
                component.Dispose();
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
        DetachComponent(component);
        component.Dispose();
    }

    public void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Entity));
    }

    public void Dispose()
    {
        ThrowIfDisposed();

        PreDisposed?.Invoke(this);

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
        {
            DestroyComponent(component);
        }

        Disposed?.Invoke(this);
        _disposed = true;
    }

    public static CancellationToken CreateCancelationToken(Entity entity)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        void handleDisposed(Entity entity)
        {
            cancellationTokenSource.Cancel();
            entity.Disposed -= handleDisposed;
        }
        entity.Disposed += handleDisposed;
        return cancellationTokenSource.Token;
    }
}
