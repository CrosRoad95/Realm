namespace RealmCore.Server.Modules.Vehicles.Access;

public sealed class VehicleUserAccessDto
{
    public required int Id { get; init; }
    public required int VehicleId { get; init; }
    public required int UserId { get; init; }
    public required byte AccessType { get; init; }
    public required string? CustomValue { get; init; }
    public bool IsOwner => AccessType == 0;

    [return: NotNullIfNotNull(nameof(vehicleUserAccessData))]
    public static VehicleUserAccessDto? Map(VehicleUserAccessData? vehicleUserAccessData)
    {
        if (vehicleUserAccessData == null)
            return null;

        return new VehicleUserAccessDto
        {
            Id = vehicleUserAccessData.Id,
            VehicleId = vehicleUserAccessData.VehicleId,
            UserId = vehicleUserAccessData.UserId,
            AccessType = vehicleUserAccessData.AccessType,
            CustomValue = vehicleUserAccessData.CustomValue,
        };
    }
}
