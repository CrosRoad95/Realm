namespace RealmCore.Server.Modules.Vehicles;

public interface IVehicleEventsFeature : IVehicleFeature, IEnumerable<VehicleEventDto>
{
    void AddEvent(int eventType, string? metadata = null);
}

internal sealed class VehicleEventsFeature : IVehicleEventsFeature, IUsesVehiclePersistentData
{
    private readonly object _lock = new();
    private ICollection<VehicleEventData> _vehicleEvents = [];
    private readonly IDateTimeProvider _dateTimeProvider;

    public event Action? VersionIncreased;

    public RealmVehicle Vehicle { get; init; }

    public VehicleEventsFeature(VehicleContext vehicleContext, IDateTimeProvider dateTimeProvider)
    {
        Vehicle = vehicleContext.Vehicle;
        _dateTimeProvider = dateTimeProvider;
    }

    public void AddEvent(int eventType, string? metadata = null)
    {
        lock (_lock)
        {
            _vehicleEvents.Add(new VehicleEventData
            {
                DateTime = _dateTimeProvider.Now,
                VehicleId = Vehicle.PersistentId,
                EventType = eventType,
                Metadata = metadata,
            });
        }
    }

    public IEnumerator<VehicleEventDto> GetEnumerator()
    {
        VehicleEventData[] view;
        lock (_lock)
            view = [.. _vehicleEvents];

        foreach (var notificationData in view)
        {
            yield return VehicleEventDto.Map(notificationData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Loaded(VehicleData vehicleData)
    {
        _vehicleEvents = vehicleData.VehicleEvents;
    }

    public void Unloaded()
    {

    }
}
