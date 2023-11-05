namespace RealmCore.Server.Elements;

public class RealmMarker : Marker, IComponents, ICollisionDetection
{
    public event Action<Element>? ElementEntered;
    public event Action<Element>? ElementLeft;

    public Concepts.Components Components { get; private set; }
    public CollisionSphere CollisionShape { get; private set; }
    public CollisionDetection<RealmMarker> CollisionDetection { get; private set; }
    public CollisionDetection InternalCollisionDetection => CollisionDetection;

    public RealmMarker(IServiceProvider serviceProvider, Vector3 position, MarkerType markerType, float size) : base(position, markerType)
    {
        Size = size;
        Components = new(serviceProvider, this);
        CollisionDetection = new(serviceProvider, this);
        CollisionShape = new CollisionSphere(position, size);
        CollisionShape.ElementEntered += HandleElementEntered;
        CollisionShape.ElementLeft += HandleElementLeft;
    }

    private void HandleElementLeft(Element leftElement)
    {
        ElementLeft?.Invoke(leftElement);
    }

    private void HandleElementEntered(Element enteredElement)
    {
        ElementEntered?.Invoke(enteredElement);
    }

    public void CheckElementWithin(Element element)
    {
        CollisionShape.CheckElementWithin(element);
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
}
