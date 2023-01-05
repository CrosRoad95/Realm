using Realm.Domain.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace Realm.Domain;

public class Entity
{
    public const string PlayerTag = "player";
    public const string VehicleTag = "vehicle";
    public const string BlipTag = "blip";
    public const string PickupTag = "pickup";
    public const string MarkerTag = "marker";
    public const string CollisionShape = "collisionShape";

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Tag { get; set; } = "";
    public string Name { get; set; } = "";

    private readonly List<Component> _components = new();
    public IEnumerable<Component> Components => _components;

    private readonly IServiceProvider _serviceProvider;

    public Transform Transform { get; private set; }

    public event Action<Component>? ComponentAdded;
    public event Action<Component>? ComponentRemoved;

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

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        if (component.Entity != null)
        {
            throw new Exception("Component already attached to other entity");
        }
        InjectProperties(component);
        component.Entity = this;
        _components.Add(component);
        Task.Run(async () =>
        {
            await component.Load();
            ComponentAdded?.Invoke(component);
        });
        return component;
    }
    
    public async Task<TComponent> AddComponentAsync<TComponent>(TComponent component) where TComponent : Component
    {
        if (component.Entity != null)
        {
            throw new Exception("Component already attached to other entity");
        }
        InjectProperties(component);
        component.Entity = this;
        _components.Add(component);
        await component.Load();
        ComponentAdded?.Invoke(component);
        return component;
    }

    public TComponent? GetComponent<TComponent>() where TComponent : Component
        => _components.OfType<TComponent>().FirstOrDefault();
    
    public bool HasComponent<TComponent>() where TComponent : Component
        => _components.OfType<TComponent>().Any();
    
    public bool TryGetComponent<TComponent>([NotNullWhen(true)] out TComponent? component) where TComponent : Component
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
        => DetachComponent(GetRequiredComponent<TComponent>());

    public void DetachComponent<TComponent>(TComponent component) where TComponent: Component
    {
        if (component.Entity == this)
        {
            _components.Remove(component);
            component.Entity = null!;
            ComponentRemoved?.Invoke(component);
        }
        else
            throw new InvalidOperationException("Component is not attached to this entity");
    }

    public void DestroyComponent<TComponent>(TComponent component) where TComponent: Component
    {
        component.Destroy();
        DetachComponent(component);
    }

    public bool TryDestroyComponent<TComponent>() where TComponent: Component
    {
        var component = GetComponent<TComponent>();
        if(component != null)
        {
            component.Destroy();
            DetachComponent(component);
            return true;
        }
        return false;
    }

    public void DestroyComponent<TComponent>() where TComponent: Component
    {
        var component = GetRequiredComponent<TComponent>();
        component.Destroy();
        DetachComponent(component);
    }

    public virtual void Destroy()
    {
        Destroyed?.Invoke(this);

        foreach (var component in _components.AsEnumerable().Reverse())
            DestroyComponent(component);
    }

    public override string ToString() => Name;
}
