namespace RealmCore.Server.Services.Vehicles;

internal class VehicleEventsService : IVehicleEventsService
{
    private readonly object _lock = new();
    private ICollection<VehicleEventData> _vehicleEvents = [];
    private readonly IDateTimeProvider _dateTimeProvider;

    public RealmVehicle Vehicle { get; }

    public VehicleEventsService(VehicleContext vehicleContext, IVehiclePersistanceService vehiclePersistanceService, IDateTimeProvider dateTimeProvider)
    {
        Vehicle = vehicleContext.Vehicle;
        vehiclePersistanceService.Loaded += HandleLoaded;
        _dateTimeProvider = dateTimeProvider;
    }

    private void HandleLoaded(IVehiclePersistanceService persistanceService, RealmVehicle vehicle)
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

    public IEnumerator<VehicleEventDTO> GetEnumerator()
    {
        lock (_lock)
            return new List<VehicleEventDTO>(_vehicleEvents.Select(VehicleEventDTO.Map)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
