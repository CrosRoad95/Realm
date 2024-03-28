namespace RealmCore.Server.Modules.Vehicles;

public sealed class VehicleEventDto
{
    public required int Id { get; init; }
    public required int EventType { get; init; }
    public required string? Metadata { get; init; }
    public required DateTime DateTime { get; init; }


    [return: NotNullIfNotNull(nameof(vehicleEventData))]
    public static VehicleEventDto? Map(VehicleEventData? vehicleEventData)
    {
        if (vehicleEventData == null)
            return null;

        return new()
        {
            Id = vehicleEventData.Id,
            EventType = vehicleEventData.EventType,
            Metadata = vehicleEventData.Metadata,
            DateTime = vehicleEventData.DateTime,
        };
    }
}
