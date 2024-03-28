namespace RealmCore.Server.Modules.Vehicles;

public sealed class VehicleLightInfoDto
{
    public required int Id { get; init; }
    public required ushort Model { get; init; }
    public required Vector3? Position { get; init; }
}
