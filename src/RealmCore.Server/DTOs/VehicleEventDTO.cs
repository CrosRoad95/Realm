namespace RealmCore.Server.DTOs;

public class VehicleEventDTO
{
    public int Id { get; set; }
    public int EventType { get; set; }
    public string? Metadata { get; set; }
    public DateTime DateTime { get; set; }


    [return: NotNullIfNotNull(nameof(vehicleEventData))]
    public static VehicleEventDTO? Map(VehicleEventData? vehicleEventData)
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
