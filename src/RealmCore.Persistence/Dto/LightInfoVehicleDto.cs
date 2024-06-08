namespace RealmCore.Persistence.Dto;

public sealed class LightInfoVehicleDto
{
    public required int Id { get; init; }
    public required ushort Model { get; init; }
    public required Vector3? Position { get; init; }
}
