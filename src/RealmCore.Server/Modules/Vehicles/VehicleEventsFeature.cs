namespace RealmCore.Server.Modules.Vehicles;

public interface IVehicleEventsFeature : IVehicleFeature, IEnumerable<VehicleEventDto>
{
    void AddEvent(int eventType, string? metadata = null);
}

internal sealed class VehicleEventsFeature : IVehicleEventsFeature
{
    private readonly object _lock = new();
    private ICollection<VehicleEventData> _vehicleEvents = [];
    private readonly IDateTimeProvider _dateTimeProvider;

    public RealmVehicle Vehicle { get; init; }

    public VehicleEventsFeature(VehicleContext vehicleContext, IVehiclePersistanceFeature vehiclePersistanceService, IDateTimeProvider dateTimeProvider)
    {
        Vehicle = vehicleContext.Vehicle;
        vehiclePersistanceService.Loaded += HandleLoaded;
        _dateTimeProvider = dateTimeProvider;
    }

    private void HandleLoaded(IVehiclePersistanceFeature persistanceService, RealmVehicle vehicle)
    {
        _vehicleEvents = persistanceService.VehicleData.VehicleEvents;
    }

    public void AddEvent(int eventType, string? metadata = null)
    {
        lock (_lock)
        {
            _vehicleEvents.Add(new VehicleEventData
            {
                DateTime = _dateTimeProvider.Now,
                VehicleId = Vehicle.PersistantId,
                EventType = eventType,
                Metadata = metadata,
            });
        }
    }

    public IEnumerator<VehicleEventDto> GetEnumerator()
    {
        lock (_lock)
            return new List<VehicleEventDto>(_vehicleEvents.Select(VehicleEventDto.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
