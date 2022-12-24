namespace Realm.Domain;

public class Entity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Tag { get; set; } = "";
    public string Name { get; set; } = "";

    private readonly List<Component> _components = new();
    private readonly IRPGServer _rpgServer;

    public Transform Transform { get; private set; }
    public IRPGServer Server => _rpgServer;

    public event Action<Component>? ComponentAdded;


    public event Action<Entity>? Destroyed;

    public Entity(IRPGServer rpgServer, ServicesComponent servicesComponent, string name = "")
    {
        _rpgServer = rpgServer;
        Name = name;
        Transform = new Transform(this);
        AddComponent(servicesComponent);
    }

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : Component
    {
        if (component.Entity != null)
        {
            throw new Exception("Component already attached to other entity");
        }
        component.Entity = this;
        _components.Add(component);
        Task.Run(component.Load);
        ComponentAdded?.Invoke(component);
        return component;
    }
    
    public async Task<TComponent> AddComponentAsync<TComponent>(TComponent component) where TComponent : Component
    {
        if (component.Entity != null)
        {
            throw new Exception("Component already attached to other entity");
        }
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
    
    public bool TryGetComponent<TComponent>(out TComponent component) where TComponent : Component
    {
        component = GetComponent<TComponent>();
        return component != null;
    }

    public TComponent GetRequiredComponent<TComponent>() where TComponent : Component
        => _components.OfType<TComponent>().First();

    public void RemoveComponent<TComponent>(TComponent component) where TComponent: Component
    {
        if (component.Entity == this)
        {
            _components.Remove(component);
            component.Entity = null!;
        }
        else
            throw new Exception();
    }

    public void DestroyComponent<TComponent>(TComponent component) where TComponent: Component
    {
        component.Destroy();
        RemoveComponent(component);
    }

    public IEnumerable<Component> GetComponents() => _components;

    #region Internal
    public TComponent? InternalGetComponent<TComponent>()
        => _components.OfType<TComponent>().FirstOrDefault();

    public TComponent InternalGetRequiredComponent<TComponent>()
        => _components.OfType<TComponent>().First();

    public List<Component> InternalGetComponents() => _components;

    public T GetRequiredService<T>() where T: notnull
    {
        return InternalGetRequiredComponent<ServicesComponent>().GetRequiredService<T>();
    }
    #endregion

    public virtual void Destroy()
    {
        Destroyed?.Invoke(this);

        foreach (var component in _components.AsEnumerable().Reverse())
            DestroyComponent(component);

    }

    public override string ToString() => Name;
}
