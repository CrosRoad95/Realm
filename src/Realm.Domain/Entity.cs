using Microsoft.AspNetCore.Components;
using Realm.Domain.Concepts.Objectives;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Realm.Domain;

public class Entity : IDisposable
{
    public const string PlayerTag = "player";
    public const string PedTag = "ped";
    public const string VehicleTag = "vehicle";
    public const string BlipTag = "blip";
    public const string PickupTag = "pickup";
    public const string MarkerTag = "marker";
    public const string CollisionShape = "collisionShape";
    public const string WorldObject = "worldObject";

    private bool _disposed = false;

    public string Id { get; } = Guid.NewGuid().ToString();
    public string Tag { get; set; } = "";
    public string Name { get; set; } = "";

    private readonly ReaderWriterLockSlim _componentsLock = new();
    private readonly List<Component> _components = new();
    public IReadOnlyCollection<Component> Components => _components;

    private readonly IServiceProvider _serviceProvider;

    public Transform Transform { get; private set; }

    public event Action<Component>? ComponentAdded;
    public event Action<Component>? ComponentDetached;

    public event Action<Entity>? Destroyed;

    public Entity(IServiceProvider serviceProvider, string name = "", string tag = "")
    {
        _serviceProvider = serviceProvider;
        Name = name;
        Tag = tag;
        Transform = new Transform(this);
    }

    private void InjectProperties<TComponent>(TComponent component) where TComponent : Component
    {
        ThrowIfDisposed();

        component.Entity = this;

        Action<Type, object> inject = default!;
        inject = (Type type, object obj) =>
        {
            foreach (var property in type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (property.GetCustomAttribute<InjectAttribute>() != null)
                {
                    property.SetValue(component, _serviceProvider.GetRequiredService(property.PropertyType));
                }
            }

            if (type != typeof(object))
            {
                inject(type.BaseType, obj);
            }
        };

        inject(typeof(TComponent), component);
    }

    private void InternalAddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        _componentsLock.EnterWriteLock();
        try
        {
            _components.Add(component);
        }
        finally
        {
            _componentsLock.ExitWriteLock();
        }
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
        Destroy();
        _disposed = true;
    }
}
