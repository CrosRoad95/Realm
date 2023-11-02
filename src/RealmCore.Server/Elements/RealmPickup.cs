namespace RealmCore.Server.Elements;

public class RealmPickup : Pickup, IComponents
{
    public Concepts.Components Components { get; private set; }

    private readonly List<IElementRule> _elementRules = new();

    public event Action<RealmPickup, Element>? Entered;
    public event Action<RealmPickup, Element>? Left;

    public RealmPickup(IServiceProvider serviceProvider, Vector3 position, ushort model) : base(position, model)
    {
        Components = new(serviceProvider, this);
    }

    public TComponent GetRequiredComponent<TComponent>() where TComponent : IComponent
    {
        return Components.GetRequiredComponent<TComponent>();
    }

    public bool TryDestroyComponent<TComponent>() where TComponent : IComponent
    {
        return Components.TryDestroyComponent<TComponent>();
    }

    public void DestroyComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        Components.DestroyComponent(component);
    }

    public void DestroyComponent<TComponent>() where TComponent : IComponent
    {
        Components.DestroyComponent<TComponent>();
    }

    public bool TryGetComponent<TComponent>(out TComponent component) where TComponent : IComponent
    {
        if (Components.TryGetComponent(out TComponent tempComponent))
        {
            component = tempComponent;
            return true;
        }
        component = default!;
        return false;
    }

    public bool HasComponent<TComponent>() where TComponent : IComponent
    {
        return Components.HasComponent<TComponent>();
    }

    public TComponent AddComponent<TComponent>() where TComponent : IComponent, new()
    {
        return Components.AddComponent<TComponent>();
    }

    public TComponent AddComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        return Components.AddComponent(component);
    }

    public TComponent AddComponentWithDI<TComponent>(params object[] parameters) where TComponent : IComponent
    {
        return Components.AddComponentWithDI<TComponent>(parameters);
    }

    public void AddRule(IElementRule elementRule)
    {
        _elementRules.Add(elementRule);
    }

    public void AddRule<TElementRule>() where TElementRule : IElementRule, new()
    {
        _elementRules.Add(new TElementRule());
    }

    public bool CheckRules(Element element)
    {
        foreach (var rule in _elementRules)
        {
            if (!rule.Check(element))
                return false;
        }
        return true;
    }

    internal void RelayEntered(Element element)
    {
        Entered?.Invoke(this, element);
    }

    internal void RelayLeft(Element element)
    {
        Left?.Invoke(this, element);
    }
}
