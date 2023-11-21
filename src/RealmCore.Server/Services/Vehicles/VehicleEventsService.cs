namespace RealmCore.Server.Services.Vehicles;

internal class VehicleEventsService : IVehicleEventsService
{
    private ICollection<VehicleEventData> _vehicleEvents = [];
    public RealmVehicle Vehicle { get; }

    public VehicleEventsService(VehicleContext vehicleContext, IVehiclePersistanceService vehiclePersistanceService)
    {
        Vehicle = vehicleContext.Vehicle;
        vehiclePersistanceService.Loaded += HandleLoaded;
    }

    private void HandleLoaded(IVehiclePersistanceService persistanceService, RealmVehicle vehicle)
    {
        _vehicleEvents = persistanceService.VehicleData.VehicleEvents;
    }
}
