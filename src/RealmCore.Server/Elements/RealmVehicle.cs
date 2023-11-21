namespace RealmCore.Server.Elements;

public class RealmVehicle : Vehicle, IComponents
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;

    public IServiceProvider ServiceProvider => _serviceProvider;
    public Concepts.Components Components { get; private set; }
    // TODO: refactor to something like "Vehicle Purpose"
    public bool IsPrivateVehicle { get; private set; } = false;
    public int? PrivateVehicleId { get; private set; }

    public IVehicleAccessService Access { get; private set; }
    public IVehiclePersistanceService Persistance { get; private set; }
    public RealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(model, position)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        Components = new(_serviceProvider, this);

        #region Initialize scope services
        GetRequiredService<VehicleContext>().Vehicle = this;
        Access = GetRequiredService<IVehicleAccessService>();
        Persistance = GetRequiredService<IVehiclePersistanceService>();
        #endregion
    }

    public T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();

    public object GetRequiredService(Type type) => _serviceProvider.GetRequiredService(type);

    public int GetPrivateVehicleId() => PrivateVehicleId ?? throw new InvalidOperationException();

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
            _serviceScope.Dispose();
            return true;
        }
        return false;
    }
}
