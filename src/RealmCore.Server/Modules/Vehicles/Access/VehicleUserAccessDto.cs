namespace RealmCore.Server.Modules.Vehicles.Access;

public abstract class VehicleAccessDto
{
    public required int Id { get; init; }
    public required int VehicleId { get; init; }
    public required int AccessType { get; init; }
    public required string? Metadata { get; init; }
    public T? GetMetadata<T>() where T : class => JsonHelpers.Deserialize<T>(Metadata);
}

public sealed class VehicleUserAccessDto : VehicleAccessDto
{
    public required int UserId { get; init; }
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
            Metadata = vehicleUserAccessData.Metadata,
        };
    }
}

public sealed class VehicleGroupAccessDto : VehicleAccessDto
{
    public required int GroupId { get; init; }

    [return: NotNullIfNotNull(nameof(vehicleGroupAccessData))]
    public static VehicleGroupAccessDto? Map(VehicleGroupAccessData? vehicleGroupAccessData)
    {
        if (vehicleGroupAccessData == null)
            return null;

        return new VehicleGroupAccessDto
        {
            Id = vehicleGroupAccessData.Id,
            VehicleId = vehicleGroupAccessData.VehicleId,
            GroupId = vehicleGroupAccessData.GroupId,
            AccessType = vehicleGroupAccessData.AccessType,
            Metadata = vehicleGroupAccessData.Metadata,
        };
    }
}
