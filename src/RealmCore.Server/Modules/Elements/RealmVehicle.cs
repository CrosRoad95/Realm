using RealmCore.Server.Modules.Vehicles.Persistence;

namespace RealmCore.Server.Modules.Elements;

public class RealmVehicle : Vehicle, IPersistentElement
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;
    private VehicleAccessController _accessController = VehicleDefaultAccessController.Instance;

    public IServiceProvider ServiceProvider => _serviceProvider;
    public int PersistentId => Persistence.Id;

    public IElementCustomDataFeature CustomData { get; init; } = new ElementCustomDataFeature();
    public IVehicleAccessFeature Access { get; init; }
    public IVehiclePersistenceFeature Persistence { get; init; }
    public IVehicleMileageCounterFeature MileageCounter { get; init; }
    public new IVehicleUpgradesFeature Upgrades { get; init; }
    public IVehiclePartDamageFeature PartDamage { get; init; }
    public IVehicleEnginesFeature Engines { get; init; }
    public IVehicleEventsFeature Events { get; init; }
    public IVehicleFuelFeature Fuel { get; init; }
    public IVehicleInventoryFeature Inventory { get; init; }

    public VehicleAccessController AccessController
    {
        get => _accessController; set
        {
            if(value == null)
                throw new ArgumentNullException(nameof(value));

            if (_accessController != value)
            {
                _accessController = value;
                AccessControllerChanged?.Invoke(this, _accessController, value);
            }
        }
    }

    public bool IsInMove => Velocity.LengthSquared() > 0.001f;
    public event Action<RealmVehicle, VehicleAccessController, VehicleAccessController>? AccessControllerChanged;

    public RealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(model, position)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;
        GetRequiredService<VehicleContext>().Vehicle = this;
        GetRequiredService<ElementContext>().Element = this;

        #region Initialize scope services
        Access = GetRequiredService<IVehicleAccessFeature>();
        Persistence = GetRequiredService<IVehiclePersistenceFeature>();
        MileageCounter = GetRequiredService<IVehicleMileageCounterFeature>();
        Upgrades = GetRequiredService<IVehicleUpgradesFeature>();
        PartDamage = GetRequiredService<IVehiclePartDamageFeature>();
        Engines = GetRequiredService<IVehicleEnginesFeature>();
        Events = GetRequiredService<IVehicleEventsFeature>();
        Fuel = GetRequiredService<IVehicleFuelFeature>();
        Inventory = GetRequiredService<IVehicleInventoryFeature>();
        #endregion
    }

    public T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
    public object GetRequiredService(Type type) => _serviceProvider.GetRequiredService(type);

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
