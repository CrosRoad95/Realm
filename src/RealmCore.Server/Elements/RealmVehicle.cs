namespace RealmCore.Server.Elements;

public class RealmVehicle : Vehicle, IComponents
{
    public Concepts.Components Components { get; private set; }
    // TODO: refactor to something like "Vehicle Purpose"
    public bool IsPrivateVehicle { get; private set; } = false;
    public int? PrivateVehicleId { get; private set; }

    public RealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(model, position)
    {
        Components = new(serviceProvider, this);
        Components.ComponentAdded += HandleComponentAdded;
        Components.ComponentDetached += HandleComponentDetached;
    }

    public int GetPrivateVehicleId() => PrivateVehicleId ?? throw new InvalidOperationException();

    private void HandleComponentAdded(IComponent component)
    {
        if (component is PrivateVehicleComponent privateVehicleComponent)
        {
            IsPrivateVehicle = true;
            PrivateVehicleId = privateVehicleComponent.Id;
        }
    }

    private void HandleComponentDetached(IComponent component)
    {
        if (component is PrivateVehicleComponent)
        {
            IsPrivateVehicle = false;
            PrivateVehicleId = null;
        }
    }

    public TComponent GetRequiredComponent<TComponent>() where TComponent : IComponent
    {
        return Components.GetRequiredComponent<TComponent>();
    }

    public bool TryDestroyComponent<TComponent>() where TComponent : IComponent
    {
        return Components.TryDestroyComponent<TComponent>();
    }

    public void DestroyComponent<TComponent>() where TComponent : IComponent
    {
        Components.DestroyComponent<TComponent>();
    }

    public void DestroyComponent<TComponent>(TComponent component) where TComponent : IComponent
    {
        Components.DestroyComponent(component);
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

    public override bool Destroy()
    {
        if (base.Destroy())
        {
            Components.ComponentAdded -= HandleComponentAdded;
            Components.ComponentDetached -= HandleComponentDetached;
            return true;
        }
        return false;
    }
}
