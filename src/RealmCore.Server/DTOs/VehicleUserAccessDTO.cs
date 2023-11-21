namespace RealmCore.Server.DTOs;

public class VehicleUserAccessDTO
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public int UserId { get; set; }
    public byte AccessType { get; set; }
    public bool IsOwner => AccessType == 0;
    public string? CustomValue { get; set; }

    [return: NotNullIfNotNull(nameof(vehicleUserAccessData))]
    public static VehicleUserAccessDTO? Map(VehicleUserAccessData? vehicleUserAccessData)
    {
        if (vehicleUserAccessData == null)
            return null;

        return new VehicleUserAccessDTO
        {
            Id = vehicleUserAccessData.Id,
            VehicleId = vehicleUserAccessData.VehicleId,
            UserId = vehicleUserAccessData.UserId,
            AccessType = vehicleUserAccessData.AccessType,
            CustomValue = vehicleUserAccessData.CustomValue,
        };
    }
}
