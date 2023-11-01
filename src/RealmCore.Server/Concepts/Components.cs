using System.Diagnostics.CodeAnalysis;

namespace RealmCore.Server.Concepts;


public sealed class Components : IDisposable
{
    private bool _disposed = false;

    public string Id { get; } = Guid.NewGuid().ToString();

    private readonly ReaderWriterLockSlim _componentsLock = new(LockRecursionPolicy.SupportsRecursion);
    private readonly List<IComponent> _components = new();
    private readonly IServiceProvider _serviceProvider;
    private readonly Element _element;

    public IEnumerable<IComponent> ComponentsLists
    {
        get
        {
            _componentsLock.EnterReadLock();
            var components = new List<IComponent>(_components);
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

    public event Action<IComponent>? ComponentAdded;
    public event Action<IComponent>? ComponentDetached;

    public Components(IServiceProvider serviceProvider, Element element)
    {
        _serviceProvider = serviceProvider;
        _element = element;
    }

    private void CheckCanBeAdded<TComponent>() where TComponent : IComponent
    {
        var componentUsageAttribute = typeof(TComponent).GetCustomAttribute<ComponentUsageAttribute>();
        if (componentUsageAttribute == null || componentUsageAttribute.AllowMultiple)
            return;

        if (_components.OfType<TComponent>().Any())
            throw new ComponentCanNotBeAddedException<TComponent>();
    }

    private void InternalAddComponent<TComponent>(TComponent component) where TComponent : IComponent
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

    private bool InternalTryDetachComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        if (_components.Contains(component))
        {
            if (component is IComponentLifecycle componentLifecycle)
                componentLifecycle.Detach();
            _components.Remove(component);
            return true;
        }
        return false;
    }

    public TComponent AddComponent<TComponent>() where TComponent : IComponent, new()
    {
        return AddComponent(new TComponent());
    }

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        ThrowIfDisposed();

        if (component.Element != null)
            throw new Exception("Component already attached to other element");
        component.Element = _element;

        InternalAddComponent(component);
        try
        {
            if (component is IComponentLifecycle componentLifecycle)
                componentLifecycle.Attach();
        }
        catch (Exception)
        {
            component.Element = null;
            throw;
        }

        ComponentAdded?.Invoke(component);
        return component;
    }

    public TComponent AddComponentWithDI<TComponent>(params object[] parameters) where TComponent : IComponent
    {
        return AddComponent(ActivatorUtilities.CreateInstance<TComponent>(_serviceProvider, parameters));
    }

    private TComponent? InternalGetComponent<TComponent>() where TComponent : IComponent
    {
        return _components.OfType<TComponent>().FirstOrDefault();
    }

    private IEnumerable<TComponent> InternalGetComponents<TComponent>() where TComponent : IComponent
    {
        return _components.OfType<TComponent>();
    }

    public TComponent? GetComponent<TComponent>() where TComponent : IComponent
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

    public TComponent? FindComponent<TComponent>(Func<TComponent, bool> callback) where TComponent : class, IComponent
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

    public IReadOnlyList<TComponent> GetComponents<TComponent>() where TComponent : IComponent
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

    internal bool InternalHasComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        return _components.Contains(component);
    }

    internal bool InternalHasComponent<TComponent>() where TComponent : IComponent
    {
        return _components.OfType<TComponent>().Any();
    }

    public bool HasComponent<TComponent>() where TComponent : IComponent
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

    public bool HasComponent<TComponent>(TComponent component) where TComponent : IComponent
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

    public bool HasComponent<TComponent>(Func<TComponent, bool> callback) where TComponent : IComponent
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

    public void DetachComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        ThrowIfDisposed();

        if (component.Element != null)
        {
            _componentsLock.EnterWriteLock();
            try
            {
                InternalTryDetachComponent(component);
            }
            finally
            {
                _componentsLock.ExitWriteLock();
            }
        }
    }

    public bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent component) where TComponent : IComponent
    {
        component = GetComponent<TComponent>()!;
        return component != null;
    }

    public TComponent GetRequiredComponent<TComponent>() where TComponent : IComponent
    {
        var component = InternalGetComponent<TComponent>();
        return component ?? throw new ComponentNotFoundException<TComponent>();
    }

    public void DestroyComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        ThrowIfDisposed();
        if (component is IComponentLifecycle componentLifecycle)
            componentLifecycle.Dispose();

        component.Element = null;
    }

    public bool TryDestroyComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        ThrowIfDisposed();

        _componentsLock.EnterWriteLock();
        try
        {
            if (InternalTryDetachComponent(component))
            {
                if (component is IComponentLifecycle componentLifecycle)
                    componentLifecycle.Dispose();
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

    public bool TryDestroyComponent<TComponent>() where TComponent : IComponent
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
                if (InternalTryDetachComponent(component))
                {
                    if (component is IComponentLifecycle componentLifecycle)
                        componentLifecycle.Dispose();
                    return true;
                }
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

    public void DestroyComponent<TComponent>() where TComponent : IComponent
    {
        ThrowIfDisposed();
        var component = GetRequiredComponent<TComponent>();
        if (!InternalTryDetachComponent(component))
        {
            throw new ComponentNotFoundException<TComponent>();
        }
        if (component is IComponentLifecycle componentLifecycle)
            componentLifecycle.Dispose();
    }

    public void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Component));
    }

    public void Dispose()
    {
        ThrowIfDisposed();

        List<IComponent> componentsToDestroy;
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

        _disposed = true;
    }
}
