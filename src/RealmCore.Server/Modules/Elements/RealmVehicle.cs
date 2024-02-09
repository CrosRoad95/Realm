namespace RealmCore.Server.Modules.Elements;

public class RealmVehicle : Vehicle
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;

    public IServiceProvider ServiceProvider => _serviceProvider;
    public int PersistantId => Persistance.Id;

    public IVehicleAccessFeature Access { get; private set; }
    public IVehiclePersistanceFeature Persistance { get; private set; }
    public IVehicleMileageCounterFeature MileageCounter { get; private set; }
    public new IVehicleUpgradesFeature Upgrades { get; private set; }
    public IVehiclePartDamageFeature PartDamage { get; private set; }
    public IVehicleEnginesFeature Engines { get; private set; }
    public IVehicleEventsFeature Events { get; private set; }
    public IVehicleFuelFeature Fuel { get; private set; }
    public IVehicleInventoryFeature Inventory { get; private set; }
    public VehicleAccessController AccessController { get; set; } = VehicleDefaultAccessController.Instance;

    public RealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(model, position)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;

        #region Initialize scope services
        GetRequiredService<VehicleContext>().Vehicle = this;
        Access = GetRequiredService<IVehicleAccessFeature>();
        Persistance = GetRequiredService<IVehiclePersistanceFeature>();
        MileageCounter = GetRequiredService<IVehicleMileageCounterFeature>();
        Upgrades = GetRequiredService<IVehicleUpgradesFeature>();
        PartDamage = GetRequiredService<IVehiclePartDamageFeature>();
        Engines = GetRequiredService<IVehicleEnginesFeature>();
        Events = GetRequiredService<IVehicleEventsFeature>();
        Fuel = GetRequiredService<IVehicleFuelFeature>();
        Inventory = GetRequiredService<IVehicleInventoryFeature>();
        #endregion
    }

    private T GetRequiredService<T>() where T : notnull => _serviceProvider.GetRequiredService<T>();
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
