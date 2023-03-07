namespace Realm.Persistance.DTOs;

public class VehicleModelPositionDTO
{
    public int Id { get; set; }
    public ushort Model { get; set; }
    public Vector3? Position { get; set; }
}
