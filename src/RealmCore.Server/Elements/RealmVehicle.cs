using RealmCore.Server.Services.Elements;

namespace RealmCore.Server.Elements;

public class RealmVehicle : Vehicle
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceScope _serviceScope;

    public IServiceProvider ServiceProvider => _serviceProvider;
    public int PersistantId => Persistance.Id;

    public IVehicleAccessService Access { get; private set; }
    public IVehiclePersistanceService Persistance { get; private set; }
    public IVehicleMileageCounterService MileageCounter { get; private set; }
    public new IVehicleUpgradesService Upgrades { get; private set; }
    public IVehiclePartDamageService PartDamage { get; private set; }
    public IVehicleEnginesService Engines { get; private set; }
    public IVehicleEventsService Events { get; private set; }
    public IVehicleFuelService Fuel { get; private set; }
    public IVehicleInventoryService Inventory { get; private set; }
    public VehicleAccessController AccessController { get; set; } = VehicleDefaultAccessController.Instance;

    public RealmVehicle(IServiceProvider serviceProvider, ushort model, Vector3 position) : base(model, position)
    {
        _serviceScope = serviceProvider.CreateScope();
        _serviceProvider = _serviceScope.ServiceProvider;

        #region Initialize scope services
        GetRequiredService<VehicleContext>().Vehicle = this;
        Access = GetRequiredService<IVehicleAccessService>();
        Persistance = GetRequiredService<IVehiclePersistanceService>();
        MileageCounter = GetRequiredService<IVehicleMileageCounterService>();
        Upgrades = GetRequiredService<IVehicleUpgradesService>();
        PartDamage = GetRequiredService<IVehiclePartDamageService>();
        Engines = GetRequiredService<IVehicleEnginesService>();
        Events = GetRequiredService<IVehicleEventsService>();
        Fuel = GetRequiredService<IVehicleFuelService>();
        Inventory = GetRequiredService<IVehicleInventoryService>();
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
