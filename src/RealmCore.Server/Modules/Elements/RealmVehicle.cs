namespace RealmCore.Server.Modules.Elements;

public class RealmVehicle : Vehicle, IPersistentElement
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;

    public IServiceProvider ServiceProvider => _serviceProvider;
    public int PersistentId => Persistence.Id;

    public IVehicleAccessFeature Access { get; init; }
    public IVehiclePersistenceFeature Persistence { get; init; }
    public IVehicleMileageCounterFeature MileageCounter { get; init; }
    public new IVehicleUpgradesFeature Upgrades { get; init; }
    public IVehiclePartDamageFeature PartDamage { get; init; }
    public IVehicleEnginesFeature Engines { get; init; }
    public IVehicleEventsFeature Events { get; init; }
    public IVehicleFuelFeature Fuel { get; init; }
    public IVehicleInventoryFeature Inventory { get; init; }
    public VehicleAccessController AccessController { get; set; } = VehicleDefaultAccessController.Instance;

    public RealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(model, position)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;

        #region Initialize scope services
        GetRequiredService<VehicleContext>().Vehicle = this;
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
